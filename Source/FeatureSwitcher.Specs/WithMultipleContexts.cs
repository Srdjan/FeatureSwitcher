using Contexteer;
using Contexteer.Configuration;
using FeatureSwitcher.Configuration;
using Machine.Specifications;

namespace FeatureSwitcher.Specs
{
#pragma warning disable 169
    // ReSharper disable InconsistentNaming
    public class TestConfiguration
    {
        public static IProvideBehavior Behavior(BusinessBranch context)
        {
            return AllFeatures.Enabled;
        }

        public static IProvideNaming Naming(BusinessBranch context)
        {
            return ProvideNaming.ByTypeName;
        }
    }

    public class TestFallbackConfiguration : IProvideBehavior, IProvideNaming
    {
        public bool IsEnabled(string feature)
        {
            if (feature == typeof(Simple).Name)
                return true;

            return FeatureConfiguration.For(Default.Context).Behavior.IsEnabled(feature);
        }

        public string For<TFeature>() where TFeature : IFeature
        {
            if (typeof(TFeature) == typeof(Complex))
                return typeof(Complex).FullName;

            return FeatureConfiguration.For(Default.Context).Naming.For<TFeature>();
        }
    }

    public class WithMultipleContexts : WithCleanUp
    {
        Establish ctx = () =>
        {
            Features.Are.ConfiguredBy.AppConfig().IgnoreConfigurationErrors();

            In<BusinessBranch>.Contexts.FeaturesAre().
                ConfiguredBy.Custom(TestConfiguration.Behavior).And.
                NamedBy.Custom(TestConfiguration.Naming);
        };

        Cleanup cleanup = () => In<BusinessBranch>.Contexts.FeaturesAre().
                                            HandledByDefault();

        Behaves_like<DisabledSimpleFeatureBehavior> a_disabled_feature;

        Behaves_like<EnabledSimpleFeatureInHeadquatersBehavior> an_enabled_feature_in_headquarters;
    }

    public class With_multiple_contexts_unconfigured_should_fallback_to_configured_default : WithEnabledByDefaultConfiguration
    {
        Behaves_like<EnabledSimpleFeatureBehavior> an_enabled_feature;

        Behaves_like<EnabledSimpleFeatureInHeadquatersBehavior> an_enabled_feature_in_headquarters;
    }

    public class With_multiple_contexts_unconfigured_naming_should_fallback_to_configured_default : WithCleanUp
    {
        Establish ctx = () =>
        {
            Features.Are.NamedBy.TypeName();

            In<BusinessBranch>.Contexts.FeaturesAre().
                ConfiguredBy.Custom(new TestFallbackConfiguration());
        };

        Cleanup cleanup = () => In<BusinessBranch>.Contexts.FeaturesAre().
                                            HandledByDefault();

        Behaves_like<DisabledSimpleFeatureBehavior> a_disabled_feature;

        Behaves_like<EnabledSimpleFeatureInHeadquatersBehavior> an_enabled_feature_in_headquarters;
    }

    public class Syntax_sugar : WithCleanUp
    {
        Because of = () => Features.Are.
            NamedBy.TypeName().And.
            NamedBy.TypeFullName().And.
            ConfiguredBy.AppConfig(new DefaultSection(), new FeaturesSection()).And.
            ConfiguredBy.AppConfig(new DefaultSection(), new FeaturesSection()).IgnoreConfigurationErrors().And.
            ConfiguredBy.AppConfig(new DefaultSection(), new FeaturesSection()).IgnoreConfigurationErrors().UsingConfigSectionGroup("test").And.
            AlwaysDisabled().And.
            AlwaysEnabled().And.
            NamedBy.TypeName().
            NamedBy.TypeFullName().
            ConfiguredBy.AppConfig(new DefaultSection(), new FeaturesSection()).
            NamedBy.TypeName().
            ConfiguredBy.AppConfig(new DefaultSection(), new FeaturesSection()).IgnoreConfigurationErrors().
            ConfiguredBy.AppConfig(new DefaultSection(), new FeaturesSection()).IgnoreConfigurationErrors().UsingConfigSectionGroup("test").
            NamedBy.TypeFullName().
            AlwaysDisabled().
            AlwaysEnabled().
            HandledByDefault();

        It should_not_fail = () => true.ShouldBeTrue();
    }
    // ReSharper restore InconsistentNaming
#pragma warning restore 169
}