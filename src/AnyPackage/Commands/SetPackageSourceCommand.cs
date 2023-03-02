// Copyright (c) Thomas Nieto - All Rights Reserved
// You may use, distribute and modify this code under the
// terms of the MIT license.

using System;
using System.Management.Automation;
using AnyPackage.Commands.Internal;
using AnyPackage.Provider;

namespace AnyPackage.Commands
{
    /// <summary>
    /// The Set-PackageSource command.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "PackageSource",
            SupportsShouldProcess = true,
            HelpUri = "https://go.anypackage.dev/Set-PackageSource")]
    [OutputType(typeof(PackageSourceInfo))]
    public sealed class SetPackageSourceCommand : SourceCommandBase
    {
        private const PackageProviderOperations SetSource = PackageProviderOperations.SetSource;

        /// <summary>
        /// Gets or sets the source name.
        /// </summary>
        [Parameter(Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        [ValidateNoWildcards]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source location.
        /// </summary>
        [Parameter]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        [Parameter]
        [ValidateNotNullOrEmpty]
        [ValidateProvider(SetSource)]
        [ArgumentCompleter(typeof(ProviderArgumentCompleter))]
        public override string Provider { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets if the source is trusted.
        /// </summary>
        [Parameter]
        public SwitchParameter Trusted { get; set; }

        /// <summary>
        /// Gets or sets if the command should pass objects through.
        /// </summary>
        [Parameter]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Instantiates the <c>SetPackageSource</c> class.
        /// </summary>
        public SetPackageSourceCommand()
        {
            Operation = SetSource;
        }

        /// <summary>
        /// Processes input.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (!ShouldProcess(Name))
            {
                return;
            }

            WriteVerbose($"Registering '{Name}' source.");

            string? location = MyInvocation.BoundParameters.ContainsKey(nameof(Location)) ? Location : null;
            bool? trusted = MyInvocation.BoundParameters.ContainsKey(nameof(Trusted)) ? Trusted.IsPresent : null;

            SetRequest(Name, location, trusted);

            var instances = GetInstances(Provider);

            foreach (var instance in instances)
            {
                WriteVerbose($"Calling '{instance.ProviderInfo.Name}' provider.");
                Request.ProviderInfo = instance.ProviderInfo;

                try
                {
                    instance.SetSource(Request);
                }
                catch (PipelineStoppedException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    var ex = new PackageProviderException(e.Message, e);
                    var er = new ErrorRecord(ex, "PackageProviderError", ErrorCategory.NotSpecified, Name);
                    WriteError(er);
                }

                // Only set first source found.
                if (Request.HasWriteObject)
                {
                    break;
                }
            }

            if (!Request.HasWriteObject)
            {
                var ex = new PackageProviderException("Package provider did not set the package source configuration.");
                var err = new ErrorRecord(ex, "PackageSourceFailedSet", ErrorCategory.NotSpecified, Name);
                WriteError(err);
            }
        }

        /// <summary>
        /// Sets the request property.
        /// </summary>
        protected override void SetRequest()
        {
            base.SetRequest();
            Request.PassThru = PassThru;
        }
    }
}
