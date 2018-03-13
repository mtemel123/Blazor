// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Razor;
using Xunit;

namespace Microsoft.AspNetCore.Blazor.Razor.Extensions
{
    public class BindTagHelperDescriptorProviderTest : BaseTagHelperDescriptorProviderTest
    {
        [Fact]
        public void Excecute_FindsBindTagHelperOnComponentType_CreatesDescriptor()
        {
            // Arrange
            var compilation = BaseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(@"
using System;
using Microsoft.AspNetCore.Blazor.Components;

namespace Test
{
    public class MyComponent : IComponent
    {
        public void Init(RenderHandle renderHandle) { }

        public void SetParameters(ParameterCollection parameters) { }

        public string MyProperty { get; set; }

        public Action<string> MyPropertyChanged { get; set; }
    }
}
"));

            Assert.Empty(compilation.GetDiagnostics());

            var context = TagHelperDescriptorProviderContext.Create();
            context.SetCompilation(compilation);

            // We run after component discovery and depend on the results.
            var componentProvider = new ComponentTagHelperDescriptorProvider();
            componentProvider.Execute(context);

            var provider = new BindTagHelperDescriptorProvider();

            // Act
            provider.Execute(context);

            // Assert
            var matches = GetBindTagHelpers(context);
            var bind = Assert.Single(matches);

            // These are features Bind Tags Helpers don't use. Verifying them once here and
            // then ignoring them.
            Assert.Empty(bind.AllowedChildTags);
            Assert.Null(bind.TagOutputHint);

            // These are features that are invariants of all Bind Tag Helpers. Verifying them once
            // here and then ignoring them.
            Assert.Empty(bind.Diagnostics);
            Assert.False(bind.HasErrors);
            Assert.Equal(BlazorMetadata.Bind.TagHelperKind, bind.Kind);
            Assert.Equal(BlazorMetadata.Bind.RuntimeName, bind.Metadata[TagHelperMetadata.Runtime.Name]);
            Assert.False(bind.IsDefaultKind());
            Assert.False(bind.KindUsesDefaultTagHelperRuntime());

            Assert.Equal("MyProperty", bind.Metadata[BlazorMetadata.Bind.ValueAttribute]);
            Assert.Equal("MyPropertyChanged", bind.Metadata[BlazorMetadata.Bind.ChangeHandlerAttribute]);

            Assert.Equal(
                "Binds the provided expression to the 'MyProperty' property and a change handler " +
                    "delegate to the 'MyPropertyChanged' property of the component.",
                bind.Documentation);

            // These are all trivally derived from the assembly/namespace/type name
            Assert.Equal("TestAssembly", bind.AssemblyName);
            Assert.Equal("Test.MyComponent", bind.Name);
            Assert.Equal("Test.MyComponent", bind.DisplayName);
            Assert.Equal("Test.MyComponent", bind.GetTypeName());
            
            // The tag matching rule for a bind-Component is always the component name + the attribute name
            var rule = Assert.Single(bind.TagMatchingRules);
            Assert.Empty(rule.Diagnostics);
            Assert.False(rule.HasErrors);
            Assert.Null(rule.ParentTag);
            Assert.Equal("MyComponent", rule.TagName);
            Assert.Equal(TagStructure.Unspecified, rule.TagStructure);

            var requiredAttribute = Assert.Single(rule.Attributes);
            Assert.Empty(requiredAttribute.Diagnostics);
            Assert.Equal("bind-MyProperty", requiredAttribute.DisplayName);
            Assert.Equal("bind-MyProperty", requiredAttribute.Name);
            Assert.Equal(RequiredAttributeDescriptor.NameComparisonMode.FullMatch, requiredAttribute.NameComparison);
            Assert.Null(requiredAttribute.Value);
            Assert.Equal(RequiredAttributeDescriptor.ValueComparisonMode.None, requiredAttribute.ValueComparison);

            var attribute = Assert.Single(bind.BoundAttributes);

            // Invariants
            Assert.Empty(attribute.Diagnostics);
            Assert.False(attribute.HasErrors);
            Assert.Equal(BlazorMetadata.Bind.TagHelperKind, attribute.Kind);
            Assert.False(attribute.IsDefaultKind());
            Assert.False(attribute.HasIndexer);
            Assert.Null(attribute.IndexerNamePrefix);
            Assert.Null(attribute.IndexerTypeName);
            Assert.False(attribute.IsIndexerBooleanProperty);
            Assert.False(attribute.IsIndexerStringProperty);

            Assert.Equal(
                "Binds the provided expression to the 'MyProperty' property and a change handler " +
                    "delegate to the 'MyPropertyChanged' property of the component.",
                attribute.Documentation);

            Assert.Equal("bind-MyProperty", attribute.Name);
            Assert.Equal("MyProperty", attribute.GetPropertyName());
            Assert.Equal("string Test.MyComponent.MyProperty", attribute.DisplayName);

            // Defined from the property type
            Assert.Equal("System.String", attribute.TypeName);
            Assert.True(attribute.IsStringProperty);
            Assert.False(attribute.IsBooleanProperty);
            Assert.False(attribute.IsEnum);
        }

        [Fact]
        public void Excecute_NoMatchedPropertiesOnComponent_IgnoresComponent()
        {
            // Arrange
            var compilation = BaseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(@"
using System;
using Microsoft.AspNetCore.Blazor.Components;

namespace Test
{
    public class MyComponent : IComponent
    {
        public void Init(RenderHandle renderHandle) { }

        public void SetParameters(ParameterCollection parameters) { }

        public string MyProperty { get; set; }

        public Action<string> MyPropertyChangedNotMatch { get; set; }
    }
}
"));

            Assert.Empty(compilation.GetDiagnostics());

            var context = TagHelperDescriptorProviderContext.Create();
            context.SetCompilation(compilation);

            // We run after component discovery and depend on the results.
            var componentProvider = new ComponentTagHelperDescriptorProvider();
            componentProvider.Execute(context);

            var provider = new BindTagHelperDescriptorProvider();

            // Act
            provider.Execute(context);

            // Assert
            var matches = GetBindTagHelpers(context);
            Assert.Empty(matches);
        }

        [Fact]
        public void Excecute_BindOnElement_CreatesDescriptor()
        {
            // Arrange
            var compilation = BaseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(@"
using Microsoft.AspNetCore.Blazor.Components;

namespace Test
{
    [BindElement(""div"", null, ""myprop"", ""myevent"")]
    public class BindTagHelpers
    {
    }
}
"));

            Assert.Empty(compilation.GetDiagnostics());

            var context = TagHelperDescriptorProviderContext.Create();
            context.SetCompilation(compilation);

            var provider = new BindTagHelperDescriptorProvider();

            // Act
            provider.Execute(context);

            // Assert
            var matches = GetBindTagHelpers(context);
            var bind = Assert.Single(matches);

            // These are features Bind Tags Helpers don't use. Verifying them once here and
            // then ignoring them.
            Assert.Empty(bind.AllowedChildTags);
            Assert.Null(bind.TagOutputHint);

            // These are features that are invariants of all Bind Tag Helpers. Verifying them once
            // here and then ignoring them.
            Assert.Empty(bind.Diagnostics);
            Assert.False(bind.HasErrors);
            Assert.Equal(BlazorMetadata.Bind.TagHelperKind, bind.Kind);
            Assert.Equal(BlazorMetadata.Bind.RuntimeName, bind.Metadata[TagHelperMetadata.Runtime.Name]);
            Assert.False(bind.IsDefaultKind());
            Assert.False(bind.KindUsesDefaultTagHelperRuntime());

            Assert.Equal("myprop", bind.Metadata[BlazorMetadata.Bind.ValueAttribute]);
            Assert.Equal("myevent", bind.Metadata[BlazorMetadata.Bind.ChangeHandlerAttribute]);

            Assert.Equal(
                "Binds the provided expression to the 'myprop' attribute and a change handler " +
                    "delegate to the 'myevent' attribute.",
                bind.Documentation);

            // These are all trivally derived from the assembly/namespace/type name
            Assert.Equal("TestAssembly", bind.AssemblyName);
            Assert.Equal("Test.MyComponent", bind.Name);
            Assert.Equal("Test.MyComponent", bind.DisplayName);
            Assert.Equal("Test.MyComponent", bind.GetTypeName());

            // The tag matching rule for a bind-Component is always the component name + the attribute name
            var rule = Assert.Single(bind.TagMatchingRules);
            Assert.Empty(rule.Diagnostics);
            Assert.False(rule.HasErrors);
            Assert.Null(rule.ParentTag);
            Assert.Equal("MyComponent", rule.TagName);
            Assert.Equal(TagStructure.Unspecified, rule.TagStructure);

            var requiredAttribute = Assert.Single(rule.Attributes);
            Assert.Empty(requiredAttribute.Diagnostics);
            Assert.Equal("bind-MyProperty", requiredAttribute.DisplayName);
            Assert.Equal("bind-MyProperty", requiredAttribute.Name);
            Assert.Equal(RequiredAttributeDescriptor.NameComparisonMode.FullMatch, requiredAttribute.NameComparison);
            Assert.Null(requiredAttribute.Value);
            Assert.Equal(RequiredAttributeDescriptor.ValueComparisonMode.None, requiredAttribute.ValueComparison);

            var attribute = Assert.Single(bind.BoundAttributes);

            // Invariants
            Assert.Empty(attribute.Diagnostics);
            Assert.False(attribute.HasErrors);
            Assert.Equal(BlazorMetadata.Bind.TagHelperKind, attribute.Kind);
            Assert.False(attribute.IsDefaultKind());
            Assert.False(attribute.HasIndexer);
            Assert.Null(attribute.IndexerNamePrefix);
            Assert.Null(attribute.IndexerTypeName);
            Assert.False(attribute.IsIndexerBooleanProperty);
            Assert.False(attribute.IsIndexerStringProperty);

            Assert.Equal(
                "Binds the provided expression to the 'MyProperty' property and a change handler " +
                    "delegate to the 'MyPropertyChanged' property of the component.",
                attribute.Documentation);

            Assert.Equal("bind-MyProperty", attribute.Name);
            Assert.Equal("MyProperty", attribute.GetPropertyName());
            Assert.Equal("string Test.MyComponent.MyProperty", attribute.DisplayName);

            // Defined from the property type
            Assert.Equal("System.String", attribute.TypeName);
            Assert.True(attribute.IsStringProperty);
            Assert.False(attribute.IsBooleanProperty);
            Assert.False(attribute.IsEnum);
        }


        private static TagHelperDescriptor[] GetBindTagHelpers(TagHelperDescriptorProviderContext context)
        {
            return ExcludeBuiltInComponents(context).Where(t => t.IsBindTagHelper()).ToArray();
        }
    }
}
