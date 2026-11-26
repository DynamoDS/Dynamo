#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// Dynamo compiles against the Newtonsoft.Json in Directory.Packages.props (13.0.4), but hosts such as Revit
    /// load their own copy (13.0.3 in Revit 2028 Preview, and some Revit components still use 13.0.1). A call the
    /// C# compiler binds to a member added after the baseline version, such as JToken.ToString(Formatting) or
    /// JToken.WriteTo(JsonWriter), throws MissingMethodException there (DYN-11186). Unit tests run against 13.0.4
    /// and cannot see it, so this test reads the compiled assemblies and fails when one references a
    /// Newtonsoft.Json member that the baseline assembly does not have.
    /// The baseline is 13.0.1 on purpose: it is the oldest copy a host may still load, and a member that exists
    /// there exists in 13.0.3 too. Known limits: members on generic Newtonsoft types and calls made through
    /// reflection are not checked.
    /// </summary>
    [TestFixture]
    [Category("UnitTests")]
    public sealed class NewtonsoftCompatibilityTests
    {
        private const string newtonsoftAssemblyName = "Newtonsoft.Json";

        private static string OutputFolder =>
            Path.GetDirectoryName(typeof(NewtonsoftCompatibilityTests).Assembly.Location)!;

        // Copied next to the test assembly by the Content item in DynamoCoreWpfTests.csproj.
        private static string BaselinePath => Path.Combine(OutputFolder, "newtonsoft-baseline", "Newtonsoft.Json.dll");

        // LibG findings Dynamo cannot fix, so they do not block unrelated Dynamo builds. Each entry is a line from the
        // failure message of the scan test, with a comment naming the LibG ticket. Remove it once LibG is fixed.
        private static readonly HashSet<string> knownLibGFindings = new(StringComparer.Ordinal);

        private const string incompatibleMessage =
            "These Newtonsoft.Json members are not in the 13.0.1 baseline. Dynamo compiles against a newer Newtonsoft.Json (13.0.4), " +
            "but hosts such as Revit load an older one (Revit 2028 Preview has 13.0.3, some Revit components 13.0.1), " +
            "so these calls throw MissingMethodException there. Use an overload that exists in both, for example " +
            "ToString(Formatting, Array.Empty<JsonConverter>()) instead of ToString(Formatting). ";

        [Test]
        public void WhenDynamoAssembliesAreScannedThenTheyOnlyReferenceMembersInBaselineNewtonsoft()
        {
            var missing = FindMissingFromBaseline(libG: false);

            Assert.That(missing, Is.Empty, incompatibleMessage + "Fix the Dynamo code:\n  " + FormatFindings(missing));
        }

        [Test]
        public void WhenLibGAssembliesAreScannedThenTheyOnlyReferenceMembersInBaselineNewtonsoft()
        {
            var missing = FindMissingFromBaseline(libG: true)
                .Where(f => !IsKnownLibGFinding(f, knownLibGFindings)).ToList();

            Assert.That(missing, Is.Empty, incompatibleMessage +
                "These come from the LibG package, not Dynamo code: fix them in the LibG repository and bump the package. " +
                "To unblock Dynamo meanwhile, add the finding to knownLibGFindings with the LibG ticket:\n  " + FormatFindings(missing));
        }

        private static List<string> FindMissingFromBaseline(bool libG)
        {
            var baselineIndex = LoadBaselineIndex();
            return CollectNewtonsoftReferences()
                .Where(r => IsLibGAssembly(r.Assembly) == libG && !baselineIndex.Contains(r.Use.Type, r.Use.Member))
                .Select(r => $"{r.Assembly}: {r.Use.Type}::{r.Use.Member}")
                .ToList();
        }

        private static bool IsLibGAssembly(string assembly) =>
            assembly.StartsWith("libg_", StringComparison.OrdinalIgnoreCase);

        private static string FormatFindings(IEnumerable<string> findings) =>
            string.Join("\n  ", findings.Distinct().OrderBy(m => m, StringComparer.Ordinal));

        [Test]
        public void WhenDynamoAssembliesAreScannedThenCoreAssembliesAreIncludedWithReferences()
        {
            // Guards against a scan that passes because it found nothing (for example the output folder layout or
            // the company filter changed).
            var withReferences = CollectNewtonsoftReferences().Select(r => r.Assembly).Distinct().ToList();

            foreach (var required in new[] { "DynamoCore", "DynamoCoreWpf", "DSCoreNodes" })
            {
                Assert.That(withReferences, Does.Contain(required),
                    $"The scan found no Newtonsoft.Json references in {required}.dll under '{OutputFolder}'. " +
                    $"Scanned assemblies with references: {string.Join(", ", withReferences)}");
            }
        }

        [Test]
        public void WhenBaselineIsReadThenItLacksTheOverloadsAddedIn1304()
        {
            // The baseline has ToString(Formatting, JsonConverter[]) and WriteTo(JsonWriter, JsonConverter[]) but not
            // the shorter ones. Fails if the baseline file is replaced by a 13.0.4 copy.
            var index = LoadBaselineIndex();

            Assert.That(index.Contains("Newtonsoft.Json.Linq.JToken",
                "System.String ToString(Newtonsoft.Json.Formatting,Newtonsoft.Json.JsonConverter[])"), Is.True);
            Assert.That(index.Contains("Newtonsoft.Json.Linq.JToken",
                "System.String ToString(Newtonsoft.Json.Formatting)"), Is.False);
            Assert.That(index.Contains("Newtonsoft.Json.Linq.JToken",
                "System.Void WriteTo(Newtonsoft.Json.JsonWriter)"), Is.False);
        }

        [Test]
        public void WhenMemberIsInheritedFromFrameworkTypeThenBaselineLookupFindsIt()
        {
            // JsonReaderException derives from System.Exception, which is outside the baseline assembly. A call such
            // as ex.Message must not be reported as missing.
            var index = LoadBaselineIndex();

            Assert.That(index.Contains("Newtonsoft.Json.JsonReaderException", "System.String get_Message()"), Is.True);
            Assert.That(index.Contains("Newtonsoft.Json.TypeAddedAfterBaseline", "System.String get_Message()"), Is.False);
        }

        [Test]
        public void WhenLibGFindingIsListedThenItIsIgnoredOnlyUnderLibGFolders()
        {
            const string member = "Newtonsoft.Json.Linq.JToken::System.String ToString(Newtonsoft.Json.Formatting)";
            var known = new HashSet<string>(StringComparer.Ordinal) { $"libg_100_0_0\\LibG.Foo: {member}" };

            Assert.That(IsKnownLibGFinding($"libg_100_0_0\\LibG.Foo: {member}", known), Is.True);
            // An ASM bump renames the folder; the entry must still match.
            Assert.That(IsKnownLibGFinding($"libg_101_0_0\\LibG.Foo: {member}", known), Is.True);
            // Same member in a Dynamo-owned assembly, in another LibG assembly, or in a non-libg folder that
            // happens to be listed, is still reported.
            Assert.That(IsKnownLibGFinding($"DynamoCore: {member}", known), Is.False);
            Assert.That(IsKnownLibGFinding($"libg_100_0_0\\LibG.Bar: {member}", known), Is.False);
            known.Add($"DynamoCore: {member}");
            Assert.That(IsKnownLibGFinding($"DynamoCore: {member}", known), Is.False);
        }

        // The libg_<ASM version> folder changes on every ASM bump, so compare without the version.
        private static string StripLibGVersion(string finding) =>
            Regex.Replace(finding, @"^libg_\d+(_\d+)*[\\/]", "libg\\", RegexOptions.IgnoreCase);

        private static bool IsKnownLibGFinding(string finding, HashSet<string> known) =>
            finding.StartsWith("libg_", StringComparison.OrdinalIgnoreCase) &&
            known.Any(k => StripLibGVersion(k) == StripLibGVersion(finding));

        private static BaselineIndex LoadBaselineIndex()
        {
            Assert.That(File.Exists(BaselinePath), Is.True,
                $"Baseline Newtonsoft.Json not found at '{BaselinePath}'. Restore and build the test project so its PackageDownload and Content item run.");

            using var stream = File.OpenRead(BaselinePath);
            using var pe = new PEReader(stream);
            return BaselineIndex.Build(pe.GetMetadataReader());
        }

        /// <summary>
        /// Autodesk-built, non-test managed assemblies in the output folder. Built-In Packages come from other
        /// repositories (for example DynamoMCP) and are not Dynamo's code, so they are not scanned. The libg_* folders
        /// are scanned: they ship inside Dynamo and load in the same hosts, even though the code lives in LibG.
        /// </summary>
        private static IEnumerable<string> FindDynamoAssemblies()
        {
            var skippedFolders = new[] { "Built-In Packages", "newtonsoft-baseline", "runtimes", "ref", "test_dependencies" };
            foreach (var file in Directory.EnumerateFiles(OutputFolder, "*.dll", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(OutputFolder, file);
                if (skippedFolders.Any(f => relative.StartsWith(f + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var name = Path.GetFileNameWithoutExtension(file);
                if (name.Contains("Test", StringComparison.OrdinalIgnoreCase) || name == "FFITarget")
                {
                    continue;
                }

                var company = FileVersionInfo.GetVersionInfo(file).CompanyName ?? string.Empty;
                if (company.StartsWith("Autodesk", StringComparison.OrdinalIgnoreCase))
                {
                    yield return file;
                }
            }
        }

        private static List<(string Assembly, MemberUse Use)> CollectNewtonsoftReferences()
        {
            var result = new List<(string, MemberUse)>();
            foreach (var file in FindDynamoAssemblies())
            {
                using var stream = File.OpenRead(file);
                using var pe = new PEReader(stream);
                if (!pe.HasMetadata)
                {
                    continue;
                }

                // Relative path without extension, so a finding under libg_<ASM version>\ shows which package it came from.
                var name = Path.ChangeExtension(Path.GetRelativePath(OutputFolder, file), null);
                foreach (var use in FindNewtonsoftMemberReferences(pe.GetMetadataReader()))
                {
                    result.Add((name, use));
                }
            }

            return result;
        }

        private readonly record struct MemberUse(string Type, string Member);

        private static IEnumerable<MemberUse> FindNewtonsoftMemberReferences(MetadataReader reader)
        {
            foreach (var handle in reader.MemberReferences)
            {
                var member = reader.GetMemberReference(handle);
                if (member.Parent.Kind != HandleKind.TypeReference ||
                    !IsFromNewtonsoft(reader, (TypeReferenceHandle)member.Parent))
                {
                    continue;
                }

                string type = SignatureNames.TypeName(reader, member.Parent);
                string name = reader.GetString(member.Name);
                yield return new MemberUse(type, member.GetKind() == MemberReferenceKind.Method
                    ? SignatureNames.MethodKey(name, member.DecodeMethodSignature(SignatureNames.Provider, null))
                    : name);
            }
        }

        private static bool IsFromNewtonsoft(MetadataReader reader, TypeReferenceHandle handle)
        {
            var scope = reader.GetTypeReference(handle).ResolutionScope;
            while (scope.Kind == HandleKind.TypeReference)
            {
                scope = reader.GetTypeReference((TypeReferenceHandle)scope).ResolutionScope;
            }

            return scope.Kind == HandleKind.AssemblyReference &&
                reader.GetString(reader.GetAssemblyReference((AssemblyReferenceHandle)scope).Name) == newtonsoftAssemblyName;
        }

        /// <summary>Methods and fields of the baseline assembly, with each type's members merged up its base-type chain.</summary>
        private sealed class BaselineIndex
        {
            private readonly Dictionary<string, HashSet<string>> declared = new(StringComparer.Ordinal);
            private readonly Dictionary<string, string?> baseOf = new(StringComparer.Ordinal);

            public static BaselineIndex Build(MetadataReader reader)
            {
                var index = new BaselineIndex();
                foreach (var typeHandle in reader.TypeDefinitions)
                {
                    var type = reader.GetTypeDefinition(typeHandle);
                    string typeName = SignatureNames.TypeName(reader, typeHandle);
                    var members = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var methodHandle in type.GetMethods())
                    {
                        var method = reader.GetMethodDefinition(methodHandle);
                        members.Add(SignatureNames.MethodKey(reader.GetString(method.Name),
                            method.DecodeSignature(SignatureNames.Provider, null)));
                    }

                    foreach (var fieldHandle in type.GetFields())
                    {
                        members.Add(reader.GetString(reader.GetFieldDefinition(fieldHandle).Name));
                    }

                    index.declared[typeName] = members;
                    index.baseOf[typeName] = type.BaseType.IsNil ? null : StripGenericArguments(SignatureNames.TypeName(reader, type.BaseType));
                }

                return index;
            }

            /// <summary>
            /// True when the member is declared on the type or a base type inside the baseline assembly. A type that
            /// is not in the baseline at all is reported as missing. When the base-type chain leaves the baseline
            /// assembly (System.Exception, System.Object), the member is looked up on that framework type.
            /// </summary>
            public bool Contains(string typeName, string member)
            {
                if (!declared.ContainsKey(typeName))
                {
                    return false;
                }

                for (string? current = typeName; current != null; current = baseOf.GetValueOrDefault(current))
                {
                    if (!declared.TryGetValue(current, out var members))
                    {
                        return FrameworkDeclares(current, member);
                    }

                    if (members.Contains(member))
                    {
                        return true;
                    }
                }

                return false;
            }

            private static bool FrameworkDeclares(string typeName, string member)
            {
                var type = Type.GetType(typeName);
                if (type == null)
                {
                    return false;
                }

                const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                    BindingFlags.Static | BindingFlags.FlattenHierarchy;
                foreach (var method in type.GetMethods(flags))
                {
                    var key = $"{method.ReturnType.FullName} {method.Name}({string.Join(",", method.GetParameters().Select(p => p.ParameterType.FullName))})";
                    if (key == member)
                    {
                        return true;
                    }
                }

                return type.GetFields(flags).Any(f => f.Name == member);
            }

            private static string StripGenericArguments(string name)
            {
                int open = name.IndexOf('<');
                return open < 0 ? name : name[..open];
            }
        }

        /// <summary>Renders types and method signatures as strings, ignoring which assembly a type comes from.</summary>
        private sealed class SignatureNames : ISignatureTypeProvider<string, object?>
        {
            public static readonly SignatureNames Provider = new();

            public static string MethodKey(string name, MethodSignature<string> signature)
                => $"{signature.ReturnType} {name}({string.Join(",", signature.ParameterTypes)})";

            public static string TypeName(MetadataReader reader, EntityHandle handle)
            {
                switch (handle.Kind)
                {
                    case HandleKind.TypeDefinition:
                        var definition = reader.GetTypeDefinition((TypeDefinitionHandle)handle);
                        var declaring = definition.GetDeclaringType();
                        return declaring.IsNil
                            ? Qualify(reader, definition.Namespace, definition.Name)
                            : TypeName(reader, declaring) + "+" + reader.GetString(definition.Name);
                    case HandleKind.TypeReference:
                        var reference = reader.GetTypeReference((TypeReferenceHandle)handle);
                        return reference.ResolutionScope.Kind == HandleKind.TypeReference
                            ? TypeName(reader, (TypeReferenceHandle)reference.ResolutionScope) + "+" + reader.GetString(reference.Name)
                            : Qualify(reader, reference.Namespace, reference.Name);
                    case HandleKind.TypeSpecification:
                        return reader.GetTypeSpecification((TypeSpecificationHandle)handle).DecodeSignature(Provider, null);
                    default:
                        return handle.Kind.ToString();
                }
            }

            private static string Qualify(MetadataReader reader, StringHandle ns, StringHandle name)
                => ns.IsNil ? reader.GetString(name) : reader.GetString(ns) + "." + reader.GetString(name);

            public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode switch
            {
                PrimitiveTypeCode.Boolean => "System.Boolean",
                PrimitiveTypeCode.Byte => "System.Byte",
                PrimitiveTypeCode.SByte => "System.SByte",
                PrimitiveTypeCode.Char => "System.Char",
                PrimitiveTypeCode.Int16 => "System.Int16",
                PrimitiveTypeCode.UInt16 => "System.UInt16",
                PrimitiveTypeCode.Int32 => "System.Int32",
                PrimitiveTypeCode.UInt32 => "System.UInt32",
                PrimitiveTypeCode.Int64 => "System.Int64",
                PrimitiveTypeCode.UInt64 => "System.UInt64",
                PrimitiveTypeCode.Single => "System.Single",
                PrimitiveTypeCode.Double => "System.Double",
                PrimitiveTypeCode.IntPtr => "System.IntPtr",
                PrimitiveTypeCode.UIntPtr => "System.UIntPtr",
                PrimitiveTypeCode.String => "System.String",
                PrimitiveTypeCode.Object => "System.Object",
                PrimitiveTypeCode.Void => "System.Void",
                PrimitiveTypeCode.TypedReference => "System.TypedReference",
                _ => typeCode.ToString(),
            };

            public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) => TypeName(reader, handle);

            public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) => TypeName(reader, handle);

            public string GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
                => reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);

            public string GetSZArrayType(string elementType) => elementType + "[]";

            public string GetArrayType(string elementType, ArrayShape shape) => elementType + "[" + new string(',', shape.Rank - 1) + "]";

            public string GetByReferenceType(string elementType) => elementType + "&";

            public string GetPointerType(string elementType) => elementType + "*";

            public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments)
                => genericType + "<" + string.Join(",", typeArguments) + ">";

            public string GetGenericTypeParameter(object? genericContext, int index) => "!" + index;

            public string GetGenericMethodParameter(object? genericContext, int index) => "!!" + index;

            public string GetFunctionPointerType(MethodSignature<string> signature) => "fnptr";

            public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType;

            public string GetPinnedType(string elementType) => elementType;
        }
    }
}
