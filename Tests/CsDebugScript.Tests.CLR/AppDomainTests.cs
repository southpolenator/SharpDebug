using CsDebugScript.CLR;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace CsDebugScript.Tests.CLR
{
    [Collection("CLR AppDomains")]
    [Trait("x64", "true")]
    [Trait("x86", "true")]
    public class AppDomainTests
    {
        [Fact]
        public void ModuleDomainTest()
        {
            IClrRuntime runtime = Process.Current.ClrRuntimes.Single();

            IClrAppDomain appDomainExe = runtime.GetAppDomainByName("AppDomains.exe");
            IClrAppDomain nestedDomain = runtime.GetAppDomainByName("Second AppDomain");

            IClrModule mscorlib = runtime.GetModuleByFileName("mscorlib.dll");
            AssertModuleContainsDomains(mscorlib, runtime.SharedDomain, appDomainExe, nestedDomain);
            AssertModuleDoesntContainDomains(mscorlib, runtime.SystemDomain);

            IClrModule appDomainsExeModule = runtime.GetModuleByFileName("AppDomains.exe");
            AssertModuleContainsDomains(appDomainsExeModule, appDomainExe);
            AssertModuleDoesntContainDomains(appDomainsExeModule, runtime.SystemDomain, runtime.SharedDomain, nestedDomain);

            IClrModule nestedExeModule = runtime.GetModuleByFileName("NestedException.exe");
            AssertModuleContainsDomains(nestedExeModule, nestedDomain);
            AssertModuleDoesntContainDomains(nestedExeModule, runtime.SystemDomain, runtime.SharedDomain, appDomainExe);
        }

        [Fact]
        public void AppDomainPropertyTest()
        {
            IClrRuntime runtime = Process.Current.ClrRuntimes.Single();

            IClrAppDomain systemDomain = runtime.SystemDomain;
            Assert.Equal("System Domain", systemDomain.Name);

            IClrAppDomain sharedDomain = runtime.SharedDomain;
            Assert.Equal("Shared Domain", sharedDomain.Name);

            Assert.Null(systemDomain.ApplicationBase);
            Assert.True(string.IsNullOrEmpty(systemDomain.ConfigurationFile));
            Assert.Equal("0: System Domain", systemDomain.ToString());

            Assert.Equal(2, runtime.AppDomains.Length);

            IClrAppDomain AppDomainsExe = runtime.AppDomains[0];
            Assert.Equal("AppDomains.exe", AppDomainsExe.Name);
            Assert.Equal(1, AppDomainsExe.Id);

            IClrAppDomain NestedExceptionExe = runtime.AppDomains[1];
            Assert.Equal("Second AppDomain", NestedExceptionExe.Name);
            Assert.Equal(2, NestedExceptionExe.Id);
        }

        [Fact]
        public void SystemAndSharedLibraryModulesTest()
        {
            IClrRuntime runtime = Process.Current.ClrRuntimes.Single();

            IClrAppDomain systemDomain = runtime.SystemDomain;
            Assert.Empty(systemDomain.Modules);

            IClrAppDomain sharedDomain = runtime.SharedDomain;
            Assert.Single(sharedDomain.Modules);

            IClrModule mscorlib = sharedDomain.Modules.Single();
            Assert.Equal("mscorlib.dll", Path.GetFileName(mscorlib.Module.ImageName), ignoreCase:true);
        }

        [Fact]
        public void ModuleAppDomainEqualityTest()
        {
            IClrRuntime runtime = Process.Current.ClrRuntimes.Single();

            IClrAppDomain appDomainsExe = runtime.GetAppDomainByName("AppDomains.exe");
            IClrAppDomain nestedExceptionExe = runtime.GetAppDomainByName("Second AppDomain");

            Dictionary<string, Module> appDomainsModules = GetAppDomainModuleDictionary(appDomainsExe);

            Assert.True(appDomainsModules.ContainsKey("appdomains.exe"));
            Assert.True(appDomainsModules.ContainsKey("mscorlib.dll"));

            Assert.False(appDomainsModules.ContainsKey("nestedexception.exe"));

            Dictionary<string, Module> nestedExceptionModules = GetAppDomainModuleDictionary(nestedExceptionExe);

            Assert.True(nestedExceptionModules.ContainsKey("nestedexception.exe"));
            Assert.True(nestedExceptionModules.ContainsKey("mscorlib.dll"));

            Assert.False(nestedExceptionModules.ContainsKey("appdomains.exe"));

            // Ensure that we use the same Module in each AppDomain.
            Assert.Equal(appDomainsModules["mscorlib.dll"], nestedExceptionModules["mscorlib.dll"]);
        }

        private static Dictionary<string, Module> GetAppDomainModuleDictionary(IClrAppDomain domain)
        {
            Dictionary<string, Module> result = new Dictionary<string, Module>(System.StringComparer.OrdinalIgnoreCase);

            foreach (Module module in domain.Modules.Select(m => m.Module))
            {
                result.Add(Path.GetFileName(module.ImageName), module);
            }
            return result;
        }

        private static void AssertModuleDoesntContainDomains(IClrModule module, params IClrAppDomain[] domainList)
        {
            foreach (IClrAppDomain domain in domainList)
            {
                Assert.DoesNotContain(module, domain.Modules);
            }
        }

        private static void AssertModuleContainsDomains(IClrModule module, params IClrAppDomain[] domainList)
        {
            foreach (IClrAppDomain domain in domainList)
            {
                Assert.Contains(module, domain.Modules);
            }
            Assert.Equal(domainList.Length, domainList[0].Runtime.AllAppDomains.Count(ad => ad.Modules.Contains(module)));
        }
    }
}
