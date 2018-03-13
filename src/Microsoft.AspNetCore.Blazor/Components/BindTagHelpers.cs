// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Blazor.Components
{
    /// <summary>
    /// Infrastructure for the discovery of <c>bind</c> attributes for markup elements.
    /// </summary>
    /// <remarks>
    /// To extend the set of <c>bind</c> attributes, define a public class named 
    /// <c>BindTagHelpers</c> and annotate it with the appropriate attributes.
    /// </remarks>
    [BindInputElement(null, null, "value", "changed")]
    [BindInputElement("checkbox", null, "checked", "changed")]
    [BindInputElement("text", null, "value", "changed")]
    public static class BindTagHelpers
    {
    }
}
