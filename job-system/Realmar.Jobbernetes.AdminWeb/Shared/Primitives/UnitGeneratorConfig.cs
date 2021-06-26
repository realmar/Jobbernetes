using UnitGenerator;

namespace Realmar.Jobbernetes.AdminWeb.Shared.Primitives
{
    internal static class UnitGeneratorConfig
    {
        public const UnitGenerateOptions Options = UnitGenerateOptions.MessagePackFormatter
                                                 | UnitGenerateOptions.ImplicitOperator
                                                 | UnitGenerateOptions.Comparable
                                                 | UnitGenerateOptions.ArithmeticOperator;
    }
}
