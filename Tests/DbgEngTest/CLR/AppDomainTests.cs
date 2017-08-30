using CsDebugScript;
using CsDebugScript.CLR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DbgEngTest.CLR
{
    [TestClass]
    [DeploymentItem(@"CLR\Apps\NestedException.cs", @"CLR\Apps")]
    [DeploymentItem(@"CLR\Apps\AppDomains.cs", @"CLR\Apps")]
    [DeploymentItem(@"CLR\Apps\SharedLibrary.cs", @"CLR\Apps")]
    public class AppDomainTests : ClrTestBase
    {
        [ClassInitialize]
        public static void TestSetup(TestContext context)
        {
            SyncStart();
            CompileAndInitialize(ClrTestApps.NestedException);
            CompileAndInitialize(ClrTestApps.AppDomains);
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            SyncStop();
        }

        [TestMethod]
        [TestCategory("CLR")]
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

        [TestMethod]
        [TestCategory("CLR")]
        public void AppDomainPropertyTest()
        {
            IClrRuntime runtime = Process.Current.ClrRuntimes.Single();

            IClrAppDomain systemDomain = runtime.SystemDomain;
            Assert.AreEqual("System Domain", systemDomain.Name);

            IClrAppDomain sharedDomain = runtime.SharedDomain;
            Assert.AreEqual("Shared Domain", sharedDomain.Name);

            Assert.AreEqual(null, systemDomain.ApplicationBase);
            Assert.IsTrue(string.IsNullOrEmpty(systemDomain.ConfigurationFile));
            Assert.AreEqual("0: System Domain", systemDomain.ToString());

            Assert.AreEqual(2, runtime.AppDomains.Length);

            IClrAppDomain AppDomainsExe = runtime.AppDomains[0];
            Assert.AreEqual("AppDomains.exe", AppDomainsExe.Name);
            Assert.AreEqual(1, AppDomainsExe.Id);

            IClrAppDomain NestedExceptionExe = runtime.AppDomains[1];
            Assert.AreEqual("Second AppDomain", NestedExceptionExe.Name);
            Assert.AreEqual(2, NestedExceptionExe.Id);
        }

        [TestMethod]
        [TestCategory("CLR")]
        public void SystemAndSharedLibraryModulesTest()
        {
            IClrRuntime runtime = Process.Current.ClrRuntimes.Single();

            IClrAppDomain systemDomain = runtime.SystemDomain;
            Assert.AreEqual(0, systemDomain.Modules.Length);

            IClrAppDomain sharedDomain = runtime.SharedDomain;
            Assert.AreEqual(1, sharedDomain.Modules.Length);

            IClrModule mscorlib = sharedDomain.Modules.Single();
            Assert.IsTrue(Path.GetFileName(mscorlib.Module.ImageName).Equals("mscorlib.dll", System.StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        [TestCategory("CLR")]
        public void ModuleAppDomainEqualityTest()
        {
            IClrRuntime runtime = Process.Current.ClrRuntimes.Single();

            IClrAppDomain appDomainsExe = runtime.GetAppDomainByName("AppDomains.exe");
            IClrAppDomain nestedExceptionExe = runtime.GetAppDomainByName("Second AppDomain");

            Dictionary<string, Module> appDomainsModules = GetAppDomainModuleDictionary(appDomainsExe);

            Assert.IsTrue(appDomainsModules.ContainsKey("appdomains.exe"));
            Assert.IsTrue(appDomainsModules.ContainsKey("mscorlib.dll"));

            Assert.IsFalse(appDomainsModules.ContainsKey("nestedexception.exe"));

            Dictionary<string, Module> nestedExceptionModules = GetAppDomainModuleDictionary(nestedExceptionExe);

            Assert.IsTrue(nestedExceptionModules.ContainsKey("nestedexception.exe"));
            Assert.IsTrue(nestedExceptionModules.ContainsKey("mscorlib.dll"));

            Assert.IsFalse(nestedExceptionModules.ContainsKey("appdomains.exe"));

            // Ensure that we use the same Module in each AppDomain.
            Assert.AreEqual(appDomainsModules["mscorlib.dll"], nestedExceptionModules["mscorlib.dll"]);
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
                Assert.IsFalse(domain.Modules.Contains(module));
            }
        }

        private static void AssertModuleContainsDomains(IClrModule module, params IClrAppDomain[] domainList)
        {
            foreach (IClrAppDomain domain in domainList)
            {
                Assert.IsTrue(domain.Modules.Contains(module));
            }
            Assert.AreEqual(domainList.Length, domainList[0].Runtime.AllAppDomains.Count(ad => ad.Modules.Contains(module)));
        }
    }
}
