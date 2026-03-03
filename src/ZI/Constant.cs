using System;
using System.Collections.Generic;
using System.Text;
using MyLibraries.UsefulMethods;

namespace ZevviCompiler.ZI
{
    public readonly struct Constant<T> : IOperand
    {
        //public readonly int size;
        public readonly T value;

        public Constant(/*int size, */T value)
        {
            //this.size = size;
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is Constant<T> constant &&
                   //size == constant.size &&
                   EqualityComparer<T>.Default.Equals(value, constant.value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(/*size, */value);
        }

        public string ToStringFull()
        {
            return value.Repr();
        }

        public string ToStringSimple()
        {
            return ToStringFull();
        }

        public override string ToString()
        {
            return ToStringFull();
        }

        dynamic IOperand.GetValue(Interpreter interpreter, int index)
        {
            if (index != 0)
            {
                throw new ZIException($"Cannot index constant in ZI code: {value}.");
            }
            return value;
        }

        dynamic IOperand.SetValue(Interpreter interpreter, dynamic value, int index)
        {
            throw new ZIException($"Cannot assign to constant in ZI code: {value}.");
        }

        public static bool operator ==(Constant<T> left, Constant<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Constant<T> left, Constant<T> right)
        {
            return !(left == right);
        }
    }
}
