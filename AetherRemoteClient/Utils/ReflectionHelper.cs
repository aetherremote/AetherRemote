using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AetherRemoteClient.Utils;

/// <summary>
///     Provides various methods for passing in reflection data to create Func and Actions for variable amount of arguments.
/// </summary>
/// <remarks>This is the first time Expression Trees are used in AR so expect the methods to be less-than-optimal.</remarks>
public static class ReflectionHelper
{
    /// <summary>
    ///     Creates a closed func delegate from provided <see cref="MethodInfo"/>
    /// </summary>
    public static Func<TResult>? CreateFunc<TResult>(object? instance, MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (parameters.Length is not 0)
            return null;
        
        Expression call;
        if (method.IsStatic)
        {
            call = Expression.Call(method);
        }
        else
        {
            if (instance is null || method.DeclaringType is not { } declaringType)
                return null;

            call = Expression.Call(Expression.Convert(Expression.Constant(instance), declaringType), method);
        }
        
        return Expression.Lambda<Func<TResult>>(Expression.Convert(call, typeof(TResult))).Compile();
    }
    
    /// <summary>
    ///     <inheritdoc cref="CreateFunc{TResult}"/>
    /// </summary>
    public static Func<T1, TResult>? CreateFunc<T1, TResult>(object? instance, MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (parameters.Length is not 1)
            return null;
        
        var delegateArgument1 = Expression.Parameter(typeof(T1), "delegateArgument1");
        
        var methodArgument1 = Expression.Convert(delegateArgument1, parameters[0].ParameterType);
        
        Expression call;
        if (method.IsStatic)
        {
            call = Expression.Call(method, methodArgument1);
        }
        else
        {
            if (instance is null || method.DeclaringType is not { } declaringType)
                return null;

            call = Expression.Call(Expression.Convert(Expression.Constant(instance), declaringType), method, methodArgument1);
        }
        
        return Expression.Lambda<Func<T1, TResult>>(Expression.Convert(call, typeof(TResult)), delegateArgument1).Compile();
    }
    
    /// <summary>
    ///     <inheritdoc cref="CreateFunc{TResult}"/>
    /// </summary>
    public static Func<T1, T2, TResult>? CreateFunc<T1, T2, TResult>(object? instance, MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (parameters.Length is not 2)
            return null;
        
        var delegateArgument1 = Expression.Parameter(typeof(T1), "delegateArgument1");
        var delegateArgument2 = Expression.Parameter(typeof(T2), "delegateArgument2");
        
        var methodArgument1 = Expression.Convert(delegateArgument1, parameters[0].ParameterType);
        var methodArgument2 = Expression.Convert(delegateArgument2, parameters[1].ParameterType);
        
        Expression call;
        if (method.IsStatic)
        {
            call = Expression.Call(method, methodArgument1, methodArgument2);
        }
        else
        {
            if (instance is null || method.DeclaringType is not { } declaringType)
                return null;

            call = Expression.Call(Expression.Convert(Expression.Constant(instance), declaringType), method, methodArgument1, methodArgument2);
        }
        
        return Expression.Lambda<Func<T1, T2, TResult>>(Expression.Convert(call, typeof(TResult)), delegateArgument1, delegateArgument2).Compile();
    }
    
    /// <summary>
    ///     <inheritdoc cref="CreateFunc{TResult}"/>
    /// </summary>
    public static Func<T1, T2, T3, TResult>? CreateFunc<T1, T2, T3, TResult>(object? instance, MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (parameters.Length is not 3)
            return null;
        
        var delegateArgument1 = Expression.Parameter(typeof(T1), "delegateArgument1");
        var delegateArgument2 = Expression.Parameter(typeof(T2), "delegateArgument2");
        var delegateArgument3 = Expression.Parameter(typeof(T3), "delegateArgument3");
        
        var methodArgument1 = Expression.Convert(delegateArgument1, parameters[0].ParameterType);
        var methodArgument2 = Expression.Convert(delegateArgument2, parameters[1].ParameterType);
        var methodArgument3 = Expression.Convert(delegateArgument3, parameters[2].ParameterType);
        
        Expression call;
        if (method.IsStatic)
        {
            call = Expression.Call(method, methodArgument1, methodArgument2, methodArgument3);
        }
        else
        {
            if (instance is null || method.DeclaringType is not { } declaringType)
                return null;

            call = Expression.Call(Expression.Convert(Expression.Constant(instance), declaringType), method, methodArgument1, methodArgument2, methodArgument3);
        }
        
        return Expression.Lambda<Func<T1, T2, T3, TResult>>(Expression.Convert(call, typeof(TResult)), delegateArgument1, delegateArgument2, delegateArgument3).Compile();
    }
    
    /// <summary>
    ///     Creates a closed action delegate from provided <see cref="MethodInfo"/>
    /// </summary>
    public static Action<T1>? CreateAction<T1>(object? instance, MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (parameters.Length is not 1)
            return null;
        
        var delegateArgument1 = Expression.Parameter(typeof(T1), "delegateArgument1");
        
        var methodArgument1 = Expression.Convert(delegateArgument1, parameters[0].ParameterType);
        
        Expression call;
        if (method.IsStatic)
        {
            call = Expression.Call(method, methodArgument1);
        }
        else
        {
            if (instance is null || method.DeclaringType is not { } declaringType)
                return null;

            call = Expression.Call(Expression.Convert(Expression.Constant(instance), declaringType), method, methodArgument1);
        }
        
        return Expression.Lambda<Action<T1>>(call, delegateArgument1).Compile();
    }
    
    /// <summary>
    ///     <inheritdoc cref="CreateAction{TResult}"/>
    /// </summary>
    public static Action<T1, T2>? CreateAction<T1, T2>(object? instance, MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (parameters.Length is not 2)
            return null;
        
        var delegateArgument1 = Expression.Parameter(typeof(T1), "delegateArgument1");
        var delegateArgument2 = Expression.Parameter(typeof(T2), "delegateArgument2");
        
        var methodArgument1 = Expression.Convert(delegateArgument1, parameters[0].ParameterType);
        var methodArgument2 = Expression.Convert(delegateArgument2, parameters[1].ParameterType);
        
        Expression call;
        if (method.IsStatic)
        {
            call = Expression.Call(method, methodArgument1, methodArgument2);
        }
        else
        {
            if (instance is null || method.DeclaringType is not { } declaringType)
                return null;

            call = Expression.Call(Expression.Convert(Expression.Constant(instance), declaringType), method, methodArgument1, methodArgument2);
        }
        
        return Expression.Lambda<Action<T1, T2>>(call, delegateArgument1, delegateArgument2).Compile();
    }
    
    /// <summary>
    ///     <inheritdoc cref="CreateAction{TResult}"/>
    /// </summary>
    public static Action<T1, T2, T3>? CreateAction<T1, T2, T3>(object? instance, MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (parameters.Length is not 3)
            return null;
        
        var delegateArgument1 = Expression.Parameter(typeof(T1), "delegateArgument1");
        var delegateArgument2 = Expression.Parameter(typeof(T2), "delegateArgument2");
        var delegateArgument3 = Expression.Parameter(typeof(T3), "delegateArgument3");
        
        var methodArgument1 = Expression.Convert(delegateArgument1, parameters[0].ParameterType);
        var methodArgument2 = Expression.Convert(delegateArgument2, parameters[1].ParameterType);
        var methodArgument3 = Expression.Convert(delegateArgument3, parameters[2].ParameterType);
        
        Expression call;
        if (method.IsStatic)
        {
            call = Expression.Call(method, methodArgument1, methodArgument2, methodArgument3);
        }
        else
        {
            if (instance is null || method.DeclaringType is not { } declaringType)
                return null;

            call = Expression.Call(Expression.Convert(Expression.Constant(instance), declaringType), method, methodArgument1, methodArgument2, methodArgument3);
        }
        
        return Expression.Lambda<Action<T1, T2, T3>>(call, delegateArgument1, delegateArgument2, delegateArgument3).Compile();
    }
    
    /// <summary>
    ///     Creates an open func to get a field from provided <see cref="FieldInfo"/>
    /// </summary>
    public static Func<object, TResult>? CreateFieldOpen<TResult>(FieldInfo fieldInfo)
    {
        if (fieldInfo.DeclaringType is not { } declaringType) return null;
        
        var instance =  Expression.Parameter(typeof(object), "instance");
        var expression = Expression.Convert(Expression.Field(Expression.Convert(instance, declaringType), fieldInfo), typeof(TResult));
        return Expression.Lambda<Func<object, TResult>>(expression, instance).Compile();
    }
    
    /// <summary>
    ///     Creates a closed func to get a field from provided <see cref="FieldInfo"/>
    /// </summary>
    public static Func<TResult>? CreateFieldClosed<TResult>(object instance, FieldInfo field)
    {
        if (field.DeclaringType is not { } declaringType) return null;
        
        var fieldAccess = Expression.Field(Expression.Convert(Expression.Constant(instance), declaringType), field);
        return Expression.Lambda<Func<TResult>>(Expression.Convert(fieldAccess, typeof(TResult))).Compile();
    }
    
    /// <summary>
    ///     Creates an open func to get a property from provided <see cref="PropertyInfo"/>
    /// </summary>
    public static Func<object, TResult>? CreateProperty<TResult>(PropertyInfo property)
    {
        if (property.DeclaringType is not { } declaringType) return null;
        
        var instance =  Expression.Parameter(typeof(object), "instance");
        var expression = Expression.Convert(Expression.Property(Expression.Convert(instance, declaringType), property), typeof(TResult));
        return Expression.Lambda<Func<object, TResult>>(expression, instance).Compile();
    }
}