using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using ToyORB.Messages;

namespace ToyORB.Proxy
{
    public interface IInvocationHandler
    {
        object MethodCall(object proxy, MethodInfo method, object[] args);
    }

    class DynamicProxy
    {
        public static T NewProxyInstance<T>(Type interfaceType, IInvocationHandler handler)
        {
            MethodInfo method;
            Random random = new Random();
            var assemblyName = new AssemblyName("ProxyAssembly_" + interfaceType.Name + "_" + random.RandomString(6));
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            var typeBuilder = moduleBuilder.DefineType("_Proxy_" + interfaceType.Name + "_" + random.RandomString(6), TypeAttributes.Public | TypeAttributes.Class);

            typeBuilder.AddInterfaceImplementation(interfaceType);
            var handlerField = typeBuilder.DefineField("_handler", typeof(IInvocationHandler), FieldAttributes.Private | FieldAttributes.Static);
            handlerField.SetValue(null, handler);

            MethodInfo methodCallInfo = typeof(IInvocationHandler).GetMethod("MethodCall");
            Type[] methodCallParamterTypes = methodCallInfo.GetParameters().Select(p => p.ParameterType).ToArray();

            foreach (var interfaceMethod in interfaceType.GetMethods())
            {
                var methodBuilder = typeBuilder.DefineMethod(interfaceMethod.Name, MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual);
                var methodParams = interfaceMethod.GetParameters().Select(p => p.ParameterType).ToArray();
                methodBuilder.SetParameters(methodParams);
                methodBuilder.SetReturnType(interfaceMethod.ReturnType);

                var ilg = methodBuilder.GetILGenerator();
                // var localMethodInfo = ilg.DeclareLocal(typeof(MethodInfo));

                // create a new array containing all arguments' values
                var argValues = ilg.DeclareLocal(typeof(object[]));
                ilg.Emit(OpCodes.Ldc_I4_S, methodParams.Length);
                ilg.Emit(OpCodes.Newarr, typeof(object));
                ilg.Emit(OpCodes.Stloc, argValues);
                for (int i = 1; i <= methodParams.Length; ++i)
                {
                    ilg.Emit(OpCodes.Ldarga, i);
                    ilg.Emit(OpCodes.Stelem_Ref);
                }
                ilg.Emit(OpCodes.Ldsfld, handlerField);  // load _handler
                ilg.Emit(OpCodes.Ldarg_0);  // load 'this' (proxy paramether of MethodCall)
                ilg.Emit(OpCodes.Ldnull);  // TODO: load MethodInfo of interface's method
                ilg.Emit(OpCodes.Callvirt, methodCallInfo, methodCallParamterTypes);
                ilg.Emit(OpCodes.Ldloca_S, (byte)0);
                ilg.Emit(OpCodes.Initobj, typeParam);
                ilg.Emit(OpCodes.Ldloc_0);
                ilg.Emit(OpCodes.Ret);
            }
            
            var generatedType = typeBuilder.CreateType();

            return null;
        }
    }
}
