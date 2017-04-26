using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ToyORB.Messages;

namespace ToyORB.Proxy
{
    public interface IInvocationHandler
    {
        object Invoke(object proxy, MethodInfo method, object[] args);
    }

    interface IExampleInterface
    {
        int method1(float arg1, string arg2, int arg3);
        void method2(int arg1);
        float method3();
    }

    class Example : IExampleInterface
    {
        private IInvocationHandler _handler;

        public Example(IInvocationHandler handler)
        {
            _handler = handler;
        }

        public int method1(float arg1, string arg2, int arg3)
        {
            MethodInfo thisMethodInfo = DynamicProxy.GetMethodByIndex(typeof(IExampleInterface).AssemblyQualifiedName, 0);
            return (int) _handler.Invoke(this, thisMethodInfo, new object[] { arg1, arg2, arg3 });
        }

        public void method2(int arg1)
        {
            MethodInfo thisMethodInfo = DynamicProxy.GetMethodByIndex(typeof(IExampleInterface).AssemblyQualifiedName, 1);
            _handler.Invoke(this, thisMethodInfo, new object[] { arg1 });
        }

        public float method3()
        {
            MethodInfo thisMethodInfo = DynamicProxy.GetMethodByIndex(typeof(IExampleInterface).AssemblyQualifiedName, 2);
            return (float) _handler.Invoke(this, thisMethodInfo, new object[] { });
        }
    }

    public class DynamicProxy
    {

        private static readonly ConcurrentDictionary<string, Type> TypesByName = new ConcurrentDictionary<string, Type>();

        public static void RegisterType(Type type)
        {
            if (type != null)
            {
                TypesByName.TryAdd(type.AssemblyQualifiedName, type);
            }
        }

        public static MethodInfo GetMethodByIndex(string assemblyQualifiedTypeName, int methodIndex)
        {
            if (TypesByName.TryGetValue(assemblyQualifiedTypeName, out Type type))
            {
                var methods = type.GetMethods();
                return methods.Length > methodIndex ? methods[methodIndex] : null;
            }

            return null;
        }

        public static MethodInfo GetPropertyAccessorByIndex(string assemblyQualifiedTypeName, int propertyIndex, int accessorIndex)
        {
            if (TypesByName.TryGetValue(assemblyQualifiedTypeName, out Type type))
            {
                var properties = type.GetProperties();
                var property = properties.Length > propertyIndex ? properties[propertyIndex] : null;

                var accessors = property?.GetAccessors();
                return accessors?.Length > accessorIndex ? accessors[accessorIndex] : null;
            }

            return null;
        }


        private static void GenerateMethod(TypeBuilder typeBuilder, FieldInfo handlerField, MethodInfo targetMethod, Action<ILGenerator> methodInfoEmitter)
        {
            var methodParams = targetMethod.GetParameters().Select(p => p.ParameterType).ToArray();

            // declare a method overriding the interface method
            var methodBuilder = typeBuilder.DefineMethod(targetMethod.Name, MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual);
            methodBuilder.SetParameters(methodParams);
            methodBuilder.SetReturnType(targetMethod.ReturnType);

            var methodIL = methodBuilder.GetILGenerator();

            // create a new array containing all arguments' values
            var argValues = methodIL.DeclareLocal(typeof(object[]));
            methodIL.Emit(OpCodes.Ldc_I4_S, methodParams.Length);  // array size for newarr
            methodIL.Emit(OpCodes.Newarr, typeof(object));  // allocate the array that will hold the argument values
            methodIL.Emit(OpCodes.Stloc, argValues);  // store in local variable argValues

            for (int i = 0; i < methodParams.Length; ++i)
            {
                methodIL.Emit(OpCodes.Ldloc, argValues);  // load reference to argValues 
                methodIL.Emit(OpCodes.Ldc_I4, i);  // load target index in argValues
                methodIL.Emit(OpCodes.Ldarg, i + 1);  // load the respective argument
                if (methodParams[i].GetTypeInfo().IsValueType)
                {
                    // box the argument value if it is a primitive (so it can be stored by ref in the object[] array)
                    methodIL.Emit(OpCodes.Box, methodParams[i]);
                }
                methodIL.Emit(OpCodes.Stelem_Ref);  // store reference (stack top) at index (stack -1) in array (stack -2)
            }

            methodIL.Emit(OpCodes.Ldarg_0);  // load this
            methodIL.Emit(OpCodes.Ldfld, handlerField);  // load this._handler

            methodIL.Emit(OpCodes.Ldarg_0);  // load 'this' (for first parameter of Invoke)
            methodInfoEmitter.Invoke(methodIL);  // load MethodInfo for second parameter of Invoke
            methodIL.Emit(OpCodes.Ldloc, argValues);  // load arguments array for third parameter of Invoke

            MethodInfo handlerInvoke = typeof(IInvocationHandler).GetMethod("Invoke");
            methodIL.Emit(OpCodes.Callvirt, handlerInvoke);  // call this._handler.Invoke(this, MethodInfo, args[])

            // Invoke will return a reference to an object, if the method returns a value type we must unbox before returning
            if (targetMethod.ReturnType.GetTypeInfo().IsValueType)
            {
                methodIL.Emit(OpCodes.Unbox, targetMethod.ReturnType);
            }

            // if the method return nothing, we must discard the result of Invoke
            if (targetMethod.ReturnType == typeof(void))
            {
                methodIL.Emit(OpCodes.Pop);
            }

            // the method implementation returns the value from Invoke
            methodIL.Emit(OpCodes.Ret);
        }

        public static T NewProxyInstance<T>(IInvocationHandler handler)
        {
            Type targetInterface = typeof(T);
            Random random = new Random();

            var assemblyName = new AssemblyName("ProxyAssembly_" + targetInterface.Name + "_" + random.RandomString(6));
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            // create a new public, sealed type that inherits from object and implements the given interface
            string proxyTypeName = "_Proxy_" + targetInterface.Name + "_" + random.RandomString(6);
            var proxyTypeAttributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed;
            var typeBuilder = moduleBuilder.DefineType(proxyTypeName, proxyTypeAttributes, typeof(object), new Type[] { targetInterface } );

            // create an instance field for the invocation handler
            typeBuilder.AddInterfaceImplementation(targetInterface);
            var handlerField = typeBuilder.DefineField("_handler", typeof(IInvocationHandler), FieldAttributes.Private);

            // build a constructor that takes the invocation handler object as the only argument
            // public _Proxy_Type(IInvocationHandler handler);
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(IInvocationHandler) });
            ILGenerator constructorIL = constructorBuilder.GetILGenerator();

            // base();
            var objectConstructor = typeof(object).GetConstructor(new Type[0]);
            constructorIL.Emit(OpCodes.Ldarg_0);  // load this
            constructorIL.Emit(OpCodes.Call, objectConstructor);  // call base constructor

            // this._handler = handler
            constructorIL.Emit(OpCodes.Ldarg_0);  // load this
            constructorIL.Emit(OpCodes.Ldarg_1);  // load first argument (handler)
            constructorIL.Emit(OpCodes.Stfld, handlerField);  // initialize handler field
            
            constructorIL.Emit(OpCodes.Ret);  // return

            Type[] interfaceTypes = Enumerable.Concat(new Type[] {targetInterface}, targetInterface.GetInterfaces()).ToArray();
            foreach (var interfaceType in interfaceTypes)
            {
                RegisterType(interfaceType);
                // for each method of the target interface, generate an implementation in the proxy type that passes the call to this._handler.Invoke()
                var interfaceMethods = interfaceType.GetMethods();
                for (int methodIndex = 0; methodIndex < interfaceMethods.Length; ++methodIndex)
                {
                    var interfaceMethod = interfaceMethods[methodIndex];
                    GenerateMethod(typeBuilder, handlerField, interfaceMethod, (methodIL) =>
                    {
                        methodIL.Emit(OpCodes.Ldstr, interfaceType.AssemblyQualifiedName);  // load type name of interface (for GetMethodByIndex)
                        methodIL.Emit(OpCodes.Ldc_I4, methodIndex);  // load index of method

                        MethodInfo getMethodByIndex = typeof(DynamicProxy).GetMethod("GetMethodByIndex");
                        methodIL.Emit(OpCodes.Call, getMethodByIndex);  // call GetMethodByIndex - results in MethodInfo for second parameter of Invoke
                    });
                }

                // also pass all property accessors through Invoke in the same way
                var interfaceProperties = interfaceType.GetProperties();
                for (int propertyIndex = 0; propertyIndex < interfaceProperties.Length; ++propertyIndex)
                {
                    var propertyAccessors = interfaceProperties[propertyIndex].GetAccessors();
                    for (int accessorIndex = 0; accessorIndex < propertyAccessors.Length; ++accessorIndex)
                    {
                        var accessorMethod = propertyAccessors[accessorIndex];
                        GenerateMethod(typeBuilder, handlerField, accessorMethod, (methodIL) =>
                        {
                            methodIL.Emit(OpCodes.Ldstr, interfaceType.AssemblyQualifiedName);  // load type name of interface (for GetMethodByIndex)
                            methodIL.Emit(OpCodes.Ldc_I4, propertyIndex);  // load index of property
                            methodIL.Emit(OpCodes.Ldc_I4, accessorIndex);  // load index of accessor in property

                            MethodInfo getPropertyAccesorByIndex = typeof(DynamicProxy).GetMethod("GetPropertyAccessorByIndex");
                            methodIL.Emit(OpCodes.Call, getPropertyAccesorByIndex);  // call GetPropertyAccessorByIndex - results in MethodInfo for second parameter of Invoke
                        });
                    }
                }
            }
            
            var proxyType = typeBuilder.CreateTypeInfo();
            var proxyConstructor = proxyType.GetConstructor(new[] {typeof(IInvocationHandler)});
            return (T) proxyConstructor.Invoke(new[] { handler });
        }
    }
}
