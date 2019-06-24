using System;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace SharpDebug.Tests
{
    public class SkippableFactMessageBus : IMessageBus
    {
        public SkippableFactMessageBus(IMessageBus innerBus, object[] constructorArguments)
        {
            InnerBus = innerBus;
            ConstructorArguments = constructorArguments;
        }

        public IMessageBus InnerBus { get; private set; }

        public int DynamicallySkippedTestCount { get; private set; }

        public object[] ConstructorArguments { get; private set; }

        public void Dispose() { }

        public bool QueueMessage(IMessageSinkMessage message)
        {
            var testFailed = message as ITestFailed;
            if (testFailed != null)
            {
                var exceptionType = testFailed.ExceptionTypes.FirstOrDefault();
                bool shouldBeSkipped = exceptionType == typeof(SkipTestException).FullName;

                if (!shouldBeSkipped)
                {
                    try
                    {
                        IAttributeInfo[] skippableAttributes = testFailed.Test.TestCase.TestMethod.Method.GetCustomAttributes(typeof(SkippableFactAttribute)).ToArray();

                        if (skippableAttributes.Length > 0)
                        {
                            var attr = skippableAttributes[0];
                            string propertyName = attr.GetNamedArgument<string>(nameof(SkippableFactAttribute.SkipOnFailurePropertyName));
                            Type testClassType = testFailed.TestClass.Class.ToRuntimeType();
                            object classInstance = Activator.CreateInstance(testClassType, ConstructorArguments);

                            for (Type type = testClassType; type != null; type = type.BaseType)
                            {
                                var property = type.GetProperty(propertyName);

                                if (property != null)
                                {
                                    shouldBeSkipped = (bool)property.GetValue(classInstance);
                                    break;
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                if (shouldBeSkipped)
                {
                    DynamicallySkippedTestCount++;
                    return InnerBus.QueueMessage(new TestSkipped(testFailed.Test, testFailed.Messages.FirstOrDefault()));
                }
            }

            // Nothing we care about, send it on its way
            return InnerBus.QueueMessage(message);
        }
    }
}
