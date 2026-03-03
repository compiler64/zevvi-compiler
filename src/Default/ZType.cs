using System;
using System.Collections.Generic;

namespace ZevviCompiler
{
    public class ZType : IType
    {
        public DefaultExtension Z { get; }

        public string Name { get; }

        private readonly int number;

        public ITransition Transition { get; set; }

        private IConverter converter;

        public IConverter Converter { get => converter; init => converter = value.Instantiate(); }

        /*public readonly ConvertDict AutoConverter;
        public readonly ConvertDict ImplicitConverter;
        public readonly ConvertDict ExplicitConverter;*/

        //public ZType(DefaultExtension z, string name, TransitionFunc transition, ConvertDict autoConverter, ConvertDict implicitConverter, ConvertDict explicitConverter)
        public ZType(DefaultExtension z, string name)
        {
            Z = z;
            Name = name;
            number = z.nextTypeNumber++;

            /*Transition = transition;
            AutoConverter = autoConverter;
            ImplicitConverter = implicitConverter;
            ExplicitConverter = explicitConverter;*/
        }

        /*public ZType(DefaultExtension z, string name, TransitionFunc transition) : this(z, name, transition, Converts.Self, Converts.None, Converts.None)
        {
        }*/

        public static ZType WithVoidConverts(DefaultExtension z, string name, ITransition transition)
        {
            return new ZType(z, name)
            {
                Transition = transition,
                Converter = z.VoidConverter,
            };
        }

        public static ZType WithNormalConverts(DefaultExtension z, string name, ITransition transition)
        {
            return new ZType(z, name)
            {
                Transition = transition,
                Converter = z.NormalConverter,
            };
        }

        /*public static ZType WithVoidLikeConverts(DefaultExtension z, string name, TransitionFunc transition)
        {
            return new ZType(z, name, transition, Converts.Self, *//*Converts.VoidLike*//*Converts.Void, Converts.None);
        }*/

        public ZI.CodeFunc AutoConvert(IType newType)
        {
            return Converter.AutoConvert.Get(this, newType);
        }

        public ZI.CodeFunc ImplicitConvert(IType newType)
        {
            return Converter.AutoConvert.Get(this, newType) ?? Converter.ImplicitConvert.Get(this, newType);
        }

        public ZI.CodeFunc ExplicitConvert(IType newType)
        {
            return Converter.AutoConvert.Get(this, newType) ?? Converter.ImplicitConvert.Get(this, newType) ?? Converter.ExplicitConvert.Get(this, newType);
        }

        public static ZI.CodeFunc[] AutoConvertList(IList<IType> oldTypes, IList<IType> newTypes)
        {
            if (oldTypes.Count != newTypes.Count)
            {
                return null;
            }

            List<ZI.CodeFunc> convertCodes = new();

            for (int i = 0; i < oldTypes.Count; i++)
            {
                ZI.CodeFunc convert = oldTypes[i].AutoConvert(newTypes[i]);

                if (convert is null)
                {
                    return null;
                }

                convertCodes.Add(convert);
            }

            return convertCodes.ToArray();
        }

        /*public static bool ImplicitConvertList(IList<ZType> oldTypes, IList<ZType> newTypes)
        {
            if (oldTypes.Count != newTypes.Count)
            {
                return false;
            }

            for (int i = 0; i < oldTypes.Count; i++)
            {
                if (oldTypes[i].ImplicitConvert(newTypes[i]) is null)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ExplicitConvertList(IList<ZType> oldTypes, IList<ZType> newTypes)
        {
            if (oldTypes.Count != newTypes.Count)
            {
                return false;
            }

            for (int i = 0; i < oldTypes.Count; i++)
            {
                if (oldTypes[i].ExplicitConvert(newTypes[i]) is null)
                {
                    return false;
                }
            }

            return true;
        }*/

        /*public MetaType As<MetaType>()
        {
            return this is MetaType metaType ? metaType : throw new ZevviInternalCompilerError($"Type {this} is not a {typeof(MetaType)}.");
        }*/

        public override bool Equals(object obj)
        {
            return obj is ZType type &&
                   number == type.number;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(number);
        }

        public override string ToString()
        {
            return Name;
        }

        public static bool operator ==(ZType left, ZType right) => left.Equals(right);

        public static bool operator !=(ZType left, ZType right) => !left.Equals(right);
    }
}
