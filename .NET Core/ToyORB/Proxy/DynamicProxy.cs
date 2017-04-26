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

            var methodIl = methodBuilder.GetILGenerator();

            // create a new array containing all arguments' values
            var argValues = methodIl.DeclareLocal(typeof(object[]));
            methodIl.Emit(OpCodes.Ldc_I4_S, methodParams.Length);  // array size for newarr
            methodIl.Emit(OpCodes.Newarr, typeof(object));  // allocate the array that will hold the argument values
            methodIl.Emit(OpCodes.Stloc, argValues);  // store in local variable argValues

            for (int i = 0; i < methodParams.Length; ++i)
            {
                methodIl.Emit(OpCodes.Ldloc, argValues);  // load reference to argValues 
                methodIl.Emit(OpCodes.Ldc_I4, i);  // load target index in argValues
                methodIl.Emit(OpCodes.Ldarg, i + 1);  // load the respective argument
                if (methodParams[i].GetTypeInfo().IsValueType)
                {
                    // box the argument value if it is a primitive (so it can be stored by ref in the object[] array)
                    methodIl.Emit(OpCodes.Box, methodParams[i]);
                }
                methodIl.Emit(OpCodes.Stelem_Ref);  // store reference (stack top) at index (stack -1) in array (stack -2)
            }

            methodIl.Emit(OpCodes.Ldarg_0);  // load this
            methodIl.Emit(OpCodes.Ldfld, handlerField);  // load this._handler

            methodIl.Emit(OpCodes.Ldarg_0);  // load 'this' (for first parameter of Invoke)
            methodInfoEmitter.Invoke(methodIl);  // load MethodInfo for second parameter of Invoke
            methodIl.Emit(OpCodes.Ldloc, argValues);  // load arguments array for third parameter of Invoke

            MethodInfo handlerInvoke = typeof(IInvocationHandler).GetMethod("Invoke");
            methodIl.Emit(OpCodes.Callvirt, handlerInvoke);  // call this._handler.Invoke(this, MethodInfo, args[])

            // Invoke will return a reference to an object, if the method returns a value type we must unbox before returning
            if (targetMethod.ReturnType.GetTypeInfo().IsValueType)
            {
                methodIl.Emit(OpCodes.Unbox, targetMethod.ReturnType);
            }

            // if the method return nothing, we must discard the result of Invoke
            if (targetMethod.ReturnType == typeof(void))
            {
                methodIl.Emit(OpCodes.Pop);
            }

            // the method implementation returns the value from Invoke
            methodIl.Emit(OpCodes.Ret);
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
            var typeBuilder = moduleBuilder.DefineType(proxyTypeName, proxyTypeAttributes, typeof(object), new[] { targetInterface } );

            // create an instance field for the invocation handler
            typeBuilder.AddInterfaceImplementation(targetInterface);
            var handlerField = typeBuilder.DefineField("_handler", typeof(IInvocationHandler), FieldAttributes.Private);

            // build a constructor that takes the invocation handler object as the only argument
            // public _Proxy_Type(IInvocationHandler handler);
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(IInvocationHandler) });
            ILGenerator constructorIl = constructorBuilder.GetILGenerator();

            // base();
            var objectConstructor = typeof(object).GetConstructor(new Type[0]);
            constructorIl.Emit(OpCodes.Ldarg_0);  // load this
            constructorIl.Emit(OpCodes.Call, objectConstructor);  // call base constructor

            // this._handler = handler
            constructorIl.Emit(OpCodes.Ldarg_0);  // load this
            constructorIl.Emit(OpCodes.Ldarg_1);  // load first argument (handler)
            constructorIl.Emit(OpCodes.Stfld, handlerField);  // initialize handler field
            
            constructorIl.Emit(OpCodes.Ret);  // return

            var interfaceTypes = new[] {targetInterface}.Concat(targetInterface.GetInterfaces()).ToArray();
            foreach (var interfaceType in interfaceTypes)
            {
                RegisterType(interfaceType);
                // for each method of the target interface, generate an implementation in the proxy type that passes the call to this._handler.Invoke()
                var interfaceMethods = interfaceType.GetMethods();
                for (int methodIndex = 0; methodIndex < interfaceMethods.Length; ++methodIndex)
                {
                    var interfaceMethod = interfaceMethods[methodIndex];
                    int methodIndexI = methodIndex;
                    GenerateMethod(typeBuilder, handlerField, interfaceMethod, (methodIl) =>
                    {
                        methodIl.Emit(OpCodes.Ldstr, interfaceType.AssemblyQualifiedName);  // load type name of interface (for GetMethodByIndex)
                        methodIl.Emit(OpCodes.Ldc_I4, methodIndexI);  // load index of method

                        MethodInfo getMethodByIndex = typeof(DynamicProxy).GetMethod("GetMethodByIndex");
                        methodIl.Emit(OpCodes.Call, getMethodByIndex);  // call GetMethodByIndex - results in MethodInfo for second parameter of Invoke
                    });
                }

                // also pass all property accessors through Invoke in the same way
                var interfaceProperties = interfaceType.GetProperties();
                for (int propertyIndex = 0; propertyIndex < interfaceProperties.Length; ++propertyIndex)
                {
                    int propertyIndexI = propertyIndex;
                    var propertyAccessors = interfaceProperties[propertyIndex].GetAccessors();
                    for (int accessorIndex = 0; accessorIndex < propertyAccessors.Length; ++accessorIndex)
                    {
                        var accessorMethod = propertyAccessors[accessorIndex];
                        int accessorIndexI = accessorIndex;
                        GenerateMethod(typeBuilder, handlerField, accessorMethod, (methodIl) =>
                        {
                            methodIl.Emit(OpCodes.Ldstr, interfaceType.AssemblyQualifiedName);  // load type name of interface (for GetMethodByIndex)
                            methodIl.Emit(OpCodes.Ldc_I4, propertyIndexI);  // load index of property
                            methodIl.Emit(OpCodes.Ldc_I4, accessorIndexI);  // load index of accessor in property

                            MethodInfo getPropertyAccesorByIndex = typeof(DynamicProxy).GetMethod("GetPropertyAccessorByIndex");
                            methodIl.Emit(OpCodes.Call, getPropertyAccesorByIndex);  // call GetPropertyAccessorByIndex - results in MethodInfo for second parameter of Invoke
                        });
                    }
                }
            }
            
            var proxyType = typeBuilder.CreateTypeInfo();
            var proxyConstructor = proxyType.GetConstructor(new[] {typeof(IInvocationHandler)});
            return (T) proxyConstructor.Invoke(new object[] { handler });
        }
    }
}
