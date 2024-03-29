// MIT License

// Copyright (c) 2022 quabug
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace GameFoundation.State
{
    public sealed class Container : IDisposable
    {
        internal readonly List<IDisposable> DisposableInstances = new List<IDisposable>();
        public void Dispose() => this.DisposeContainer();
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class InjectAttribute : Attribute { }

    public class ResolverBuilder
    {
        [NotNull] protected readonly Container _Container;
        [NotNull] protected readonly Func<Container, Type, object> _Creator;
        [NotNull] protected readonly Type _ConcreteType;

        public ResolverBuilder([NotNull] Container container, [NotNull] Type concreteType, [NotNull] Func<Container, Type, object> creator)
        {
            _Container = container;
            _Creator = creator;
            _ConcreteType = concreteType;
        }

        [NotNull]
        public ResolverBuilder As([NotNull] Type contractType)
        {
            if (!contractType.IsAssignableFrom(_ConcreteType)) throw new ArgumentException();
            var resolver = GetOrCreateResolver(contractType);
            if (!resolver.Contains(_Creator)) resolver.Insert(0, _Creator);
            return this;
        }

        [NotNull]
        public ResolverBuilder As<T>()
        {
            return As(typeof(T));
        }

        [NotNull]
        public ResolverBuilder AsSelf()
        {
            return As(_ConcreteType);
        }

        [NotNull]
        public ResolverBuilder AsInterfaces()
        {
            foreach (var @interface in _ConcreteType.GetInterfaces()) As(@interface);
            return this;
        }

        [NotNull]
        public ResolverBuilder AsBases()
        {
            var baseType = _ConcreteType.BaseType;
            while (baseType != null)
            {
                As(baseType);
                baseType = baseType.BaseType;
            }
            return this;
        }

        private List<Func<Container, Type, object>> GetOrCreateResolver(Type type)
        {
            return _Container.GetOrCreateResolver(type);
        }
    }

    public class LifetimeBuilder : ResolverBuilder
    {
        public LifetimeBuilder([NotNull] Container container, [NotNull] Func<Container, Type, object> creator, [NotNull] Type concreteType) : base(container, concreteType, creator) { }

        public ResolverBuilder Transient()
        {
            return this;
        }

        public ResolverBuilder Singleton()
        {
            var lazyValue = new Lazy<object>(() => _Creator(_Container, _ConcreteType));
            return new ResolverBuilder(_Container, _ConcreteType, (container, contractType) => lazyValue.Value);
        }

        public ResolverBuilder Scope()
        {
            var lazyValue = new Lazy<object>(() => _Creator(_Container, _ConcreteType));
            return new ResolverBuilder(_Container, _ConcreteType, ResolveScopeInstance);

            object ResolveScopeInstance(Container container, Type contractType)
            {
                if (container == _Container) return lazyValue.Value;
                // register on runtime should be thread safe?
                container.Register(_ConcreteType, _Creator).Scope().As(contractType);
                return container.Resolve(contractType);
            }
        }
    }

    public class WithBuilder : LifetimeBuilder
    {
        public WithBuilder([NotNull] Container container, [NotNull] Func<Container, Type, object> creator, [NotNull] Type concreteType) : base(container, creator, concreteType) { }

        public LifetimeBuilder With(params object[] instances)
        {
            if (instances == null || !instances.Any()) return this;
            var container = _Container.CreateChildContainer();
            foreach (var instance in instances) container.RegisterInstance(instance).AsSelf().AsBases().AsInterfaces();
            return new LifetimeBuilder(_Container, (_, contractType) => _Creator(container, contractType), _ConcreteType);
        }
    }

    public static class TypeCreatorRegister
    {
        private static readonly Dictionary<Container, Container> _containerParentMap =
            new Dictionary<Container, Container>()
        ;

        private static readonly Dictionary<Container, Dictionary<Type, List<Func<Container, Type, object>>>> _containerResolvers =
            new Dictionary<Container, Dictionary<Type, List<Func<Container, Type, object>>>>()
        ;


        [NotNull]
        public static Container CreateChildContainer([NotNull] this Container container)
        {
            var child = new Container();
            _containerParentMap[child] = container;
            return child;
        }

        [NotNull]
        public static Container BeginScope([NotNull] this Container container)
        {
            return CreateChildContainer(container);
        }

        [NotNull]
        internal static List<Func<Container, Type, object>> GetOrCreateResolver(this Container container, Type type)
        {
            return _containerResolvers.GetOrCreate(container).GetOrCreate(type);
        }

        [NotNull]
        public static object Resolve([NotNull] this Container container, [NotNull] Type type)
        {
            using (CircularCheck.Create())
            {
                var creator = container.FindFirstCreatorsInHierarchy(type);
                return creator != null ? creator(container, type) : throw new ArgumentException($"{type.Name} have not been registered yet");
            }
        }

        [NotNull]
        public static T Resolve<T>([NotNull] this Container container)
        {
            return (T)container.Resolve(typeof(T));
        }

        [NotNull]
        private static IEnumerable<object> ResolveGroupWithoutException([NotNull] this Container container, Type type)
        {
            using (CircularCheck.Create())
            {
                var creators = FindCreatorsInHierarchy(container, type).SelectMany(c => c);
                return creators.Select(creator => creator(container, type));
            }
        }

        [NotNull]
        public static IEnumerable<object> ResolveGroup([NotNull] this Container container, Type type)
        {
            var objects = container.ResolveGroupWithoutException(type);
            if (objects.Any()) return objects;
            throw new ArgumentException($"{type.Name} have not been registered into containers.");
        }

        [NotNull]
        public static IEnumerable<T> ResolveGroup<T>([NotNull] this Container container)
        {
            return container.ResolveGroup(typeof(T)).OfType<T>();
        }

        public static WithBuilder Register([NotNull] this Container container, [NotNull] Type type, [NotNull] Func<Container, Type, object> creator)
        {
            return new WithBuilder(container, creator, type);
        }

        public static WithBuilder Register<T>([NotNull] this Container container, [NotNull] Func<Container, Type, T> creator) where T : class
        {
            return container.Register(typeof(T), creator);
        }

        public static WithBuilder Register<T>([NotNull] this Container container)
        {
            return container.Register(typeof(T));
        }

        public static WithBuilder Register([NotNull] this Container container, [NotNull] Type type)
        {
            var ci = FindConstructorInfo(type);
            return container.Register(type, CreateInstance());

            Func<Container, Type, object> CreateInstance()
            {
                var parameters = ci.GetParameters();
                var arguments = new object[parameters.Length];
                return (resolveContainer, _) =>
                {
                    CircularCheck.Check(type);
                    var instance = ci.Invoke(resolveContainer.ResolveParameterInfos(parameters, arguments));
                    if (instance is IDisposable disposable) resolveContainer.DisposableInstances.Add(disposable);
                    return instance;
                };
            }
        }

        public static ResolverBuilder RegisterInstance<T>([NotNull] this Container container, [NotNull] T instance)
        {
            return new ResolverBuilder(container, instance.GetType(), (c, t) => instance);
        }

        public static void RemoveInstance<T>([NotNull] this Container container, [NotNull] T instance)
        {
            _containerResolvers.TryGetValue(container, out var resolvers);
            resolvers.Remove(instance.GetType());
        }

        // Unity/Mono: local function with default parameter is not supported by Mono?
        public static object CallFunc<T>([NotNull] this Container container, T func) where T : Delegate
        {
            using (CircularCheck.Create())
            {
                var method = func.Method;
                if (method.ReturnType == typeof(void)) throw new ArgumentException();
                return method.Invoke(func.Target, container.ResolveParameterInfos(method.GetParameters()));
            }
        }

        // Unity/Mono: local function with default parameter is not supported by Mono?
        public static void CallAction<T>([NotNull] this Container container, T action) where T : Delegate
        {
            using (CircularCheck.Create())
            {
                var method = action.Method;
                method.Invoke(action.Target, container.ResolveParameterInfos(method.GetParameters()));
            }
        }

        public static object Instantiate([NotNull] this Container container, Type type)
        {
            using (CircularCheck.Create())
            {
                var ci = FindConstructorInfo(type);
                var parameters = ci.GetParameters();
                return ci.Invoke(container.ResolveParameterInfos(parameters));
            }
        }

        public static T Instantiate<T>([NotNull] this Container container)
        {
            return (T)container.Instantiate(typeof(T));
        }

        public static void DisposeContainer([NotNull] this Container container)
        {
            var descendantsAndSelf = new HashSet<Container> { container };
            var irrelevantContainers = new HashSet<Container>();
            var containerHierarchy = new List<Container>();
            foreach (var check in _containerParentMap.Keys)
            {
                containerHierarchy.Clear();
                var containers = IsDescendant(check) ? descendantsAndSelf : irrelevantContainers;
                foreach (var c in containerHierarchy) containers.Add(c);
            }

            foreach (var disposed in descendantsAndSelf)
            {
                _containerResolvers.Remove(disposed);
                _containerParentMap.Remove(disposed);
                foreach (var instance in disposed.DisposableInstances) instance.Dispose();
                disposed.DisposableInstances.Clear();
            }

            bool IsDescendant(Container check)
            {
                containerHierarchy.Add(check);
                if (!_containerParentMap.TryGetValue(check, out var parent)) return false;
                if (descendantsAndSelf.Contains(parent)) return true;
                if (irrelevantContainers.Contains(parent)) return false;
                return IsDescendant(parent);
            }
        }

        private static ConstructorInfo FindConstructorInfo(Type type)
        {
            var constructors = type.GetConstructors();
            ConstructorInfo ci = null;
            if (constructors.Length == 1) ci = constructors[0];
            else if (constructors.Length > 1) ci = constructors.Single(c => c.GetCustomAttribute<InjectAttribute>() != null);
            if (ci == null) throw new NotSupportedException($"cannot found constructor of type {type}");
            return ci;
        }

        private static IEnumerable<List<Func<Container, Type, object>>> FindCreatorsInHierarchy(Container container, Type type)
        {
            for (; ; )
            {
                if (_containerResolvers.TryGetValue(container, out var resolvers) && resolvers.TryGetValue(type, out var creators))
                    yield return creators;
                if (!_containerParentMap.TryGetValue(container, out container)) break;
            }
        }

        private static Func<Container, Type, object> FindFirstCreatorsInHierarchy(this Container container, Type type)
        {
            do
            {
                var creators = container.GetOrCreateResolver(type);
                if (creators.Count > 0) return creators[0];
            } while (_containerParentMap.TryGetValue(container, out container));
            return null;
        }

        internal static object ResolveParameterInfo(this Container container, ParameterInfo parameter)
        {
            var parameterType = parameter.ParameterType;
            var creator = container.FindFirstCreatorsInHierarchy(parameterType);
            if (creator != null) return creator(container, parameterType);

            if (parameterType.IsArray)
            {
                var elementType = parameterType.GetElementType();
                var arrayArgument = container.ResolveGroupWithoutException(elementType);
                if (arrayArgument.Any())
                {
                    var source = arrayArgument.ToArray();
                    var dest = Array.CreateInstance(elementType, source.Length);
                    Array.Copy(source, dest, source.Length);
                    return dest;
                }
            }
            return parameter.HasDefaultValue ? parameter.DefaultValue : throw new ArgumentException($"cannot resolve parameter {parameter.Member.DeclaringType?.Name}.{parameter.Member.Name}.{parameter.Name}");
        }

        internal static object[] ResolveParameterInfos(this Container container, ParameterInfo[] parameters, object[] arguments = null)
        {
            if (arguments == null) arguments = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++) arguments[i] = ResolveParameterInfo(container, parameters[i]);
            return arguments;
        }
    }

    public static class InjectExtension
    {
        private static readonly Dictionary<Type, TypeInjector> _injectors = new Dictionary<Type, TypeInjector>();

        public static void InjectAll([NotNull] this Container container, object instance, Type instanceType)
        {
            _injectors.GetOrCreate(instanceType, () => new TypeInjector(instanceType)).InjectAll(container, instance);
        }

        public static void InjectAll<T>([NotNull] this Container container, T instance)
        {
            InjectAll(container, instance, instance.GetType());
        }
    }

    class TypeInjector
    {
        private readonly Type _type;
        private readonly List<FieldInfo> _fields = new List<FieldInfo>();
        private readonly List<PropertyInfo> _properties = new List<PropertyInfo>();
        private readonly List<MethodInfo> _methods = new List<MethodInfo>();

        public TypeInjector(Type type)
        {
            _type = type;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var member in type.GetMembers(flags).Where(mi => mi.GetCustomAttribute<InjectAttribute>() != null))
            {
                switch (member)
                {
                    case FieldInfo field:
                        _fields.Add(field);
                        break;
                    case PropertyInfo property:
                        if (property.CanWrite) _properties.Add(property);
                        else throw new NotSupportedException($"cannot inject on read-only property {property.DeclaringType.Name}.{property.Name}");
                        break;
                    case MethodInfo method: // TODO: check method validation
                        _methods.Add(method);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public void InjectFields(Container container, object instance)
        {
            CheckInstanceType(instance);
            foreach (var field in _fields) field.SetValue(instance, container.Resolve(field.FieldType));
        }

        public void InjectProperties(Container container, object instance)
        {
            CheckInstanceType(instance);
            foreach (var property in _properties) property.SetValue(instance, container.Resolve(property.PropertyType));
        }

        public void InjectMethods(Container container, object instance)
        {
            using (CircularCheck.Create())
            {
                foreach (var method in _methods) method.Invoke(instance, container.ResolveParameterInfos(method.GetParameters()));
            }
        }

        public void InjectAll(Container container, object instance)
        {
            InjectFields(container, instance);
            InjectProperties(container, instance);
            InjectMethods(container, instance);
        }

        private void CheckInstanceType(object instance)
        {
            if (instance.GetType() != _type) throw new ArgumentException();
        }
    }

    internal static class DictionaryExtension
    {
        [NotNull]
        public static TValue GetOrCreate<TKey, TValue>(
            [NotNull] this Dictionary<TKey, TValue> dictionary,
            [NotNull] TKey key
        ) where TValue : new()
        {
            return dictionary.GetOrCreate(key, () => new TValue());
        }

        [NotNull]
        public static TValue GetOrCreate<TKey, TValue>(
            [NotNull] this Dictionary<TKey, TValue> dictionary,
            [NotNull] TKey key,
            [NotNull] Func<TValue> newValue
        )
        {
            if (!dictionary.TryGetValue(key, out var value))
            {
                value = newValue();
                dictionary[key] = value;
            }
            return value;
        }
    }

    [Serializable]
    public class CircularDependencyException : Exception
    {
        public CircularDependencyException() { }
        public CircularDependencyException(string message) : base(message) { }
        public CircularDependencyException(string message, Exception inner) : base(message, inner) { }
        protected CircularDependencyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    readonly struct CircularCheck : IDisposable
    {
        [ThreadStatic] private static HashSet<Type> _circularCheckSet;

        public static CircularCheck Create()
        {
            if (_circularCheckSet == null) _circularCheckSet = new HashSet<Type>();
            _circularCheckSet.Clear();
            return new CircularCheck();
        }

        public static void Check(Type type)
        {
            if (_circularCheckSet.Contains(type)) throw new CircularDependencyException($"circular dependency on {type.Name}");
            _circularCheckSet.Add(type);
        }

        public void Dispose()
        {
            _circularCheckSet.Clear();
        }
    }
}