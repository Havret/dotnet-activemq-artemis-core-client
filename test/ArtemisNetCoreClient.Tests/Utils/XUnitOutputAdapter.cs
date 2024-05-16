using System.Text;
using Xunit.Abstractions;

namespace ActiveMQ.Artemis.Core.Client.Tests.Utils;

public class XUnitOutputAdapter(ITestOutputHelper output) : TextWriter
{
    public override void WriteLine(string? value) => output.WriteLine(value);
    public override Encoding Encoding { get; }
}