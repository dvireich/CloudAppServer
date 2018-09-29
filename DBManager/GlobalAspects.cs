using PostSharp.Patterns.Diagnostics;
using PostSharp.Extensibility;
// This file contains registration of aspects that are applied to several classes of this project.
[assembly: Log(AttributeTargetElements=MulticastTargets.Method, AttributeTargetTypeAttributes=MulticastAttributes.Public, AttributeTargetMemberAttributes=MulticastAttributes.Public)]