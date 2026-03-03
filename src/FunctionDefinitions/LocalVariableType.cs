using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MI = ZevviCompiler.MachineInstruction;

namespace ZevviCompiler.FunctionDefinitions
{
    [Obsolete("Use the more general meta-type VariableExtensions.VariableType instead.", error: true)]
    public class LocalVariableType : AbstractIdentifierType
    {
        public static readonly ConvertDict ParameterAutoConvert = new()
        {
            (oldType, newType) =>
            {
                LocalVariableType paramType = oldType.As<LocalVariableType>();
                ZI.CodeFunc convert = paramType.innerType.AutoConvert(newType);

                if (convert is null)
                {
                    return null;
                }

                /*ZI.Code local = ActivationRecords.GetLocal(paramType.paramNumber);
                ZI.Code code = local + convert(local.storage);*/
                ZI.Code code = convert(new ZI.Variable(paramType.paramNumber, ZI.VariableStorageType.Parameter));
                //return convert is null ? null : CallRecordsObsolete.PushParameter(paramType.paramNumber) + convert;
                return storage => code;
            },
            Converts.Self,
            /*(oldType, storage) => new Dictionary<ZType, ZI.Code>
            {
                [oldType] = ZI.Code.None,
                //[oldType.As<VariableType>().innerType] = new SingleMI(MI.PushI, oldType.As<VariableType>().location)
            }*/
        };

        public static readonly ConvertDict ParameterImplicitConvert = new()
        {
            Converts.Void,
            (oldType, newType) =>
            {
                LocalVariableType paramType = oldType.As<LocalVariableType>();
                ZI.CodeFunc convert = paramType.innerType.ImplicitConvert(newType);

                if (convert is null)
                {
                    return null;
                }

                /*ZI.Code local = ActivationRecords.GetLocal(paramType.paramNumber);
                ZI.Code code = local + convert(local.storage);*/
                ZI.Code code = convert(new ZI.Variable(paramType.paramNumber, ZI.VariableStorageType.Parameter));
                //return convert is null ? null : CallRecordsObsolete.PushParameter(paramType.paramNumber) + convert;
                return storage => code;
            },
        };

        public static readonly ConvertDict ParameterExplicitConvert = new()
        {
            (oldType, newType) =>
            {
                LocalVariableType paramType = oldType.As<LocalVariableType>();
                ZI.CodeFunc convert = paramType.innerType.ExplicitConvert(newType);

                if (convert is null)
                {
                    return null;
                }

                /*ZI.Code local = ActivationRecords.GetLocal(paramType.paramNumber);
                ZI.Code code = local + convert(local.storage);*/
                ZI.Code code = convert(new ZI.Variable(paramType.paramNumber, ZI.VariableStorageType.Parameter));
                //return convert is null ? null : CallRecordsObsolete.PushParameter(paramType.paramNumber) + convert;
                return convert;
            },
        };

        public readonly ZType innerType;
        public readonly int paramNumber;

        public LocalVariableType(DefaultExtension z, string identifier, ZType innerType, int paramNumber)
            : base(z, $"param {innerType}", identifier, () => z.NormalTransition($"param {innerType}"),
            ParameterAutoConvert, ParameterImplicitConvert, ParameterExplicitConvert) // variable convert is auto-converter
        {
            this.innerType = innerType;
            this.paramNumber = paramNumber;

            if (!identifier.All(c => char.IsLetterOrDigit(c)))
            {
                throw new ZevviInternalCompilerError($"Invalid variable name: '{identifier}'.");
            }
        }
    }
}
