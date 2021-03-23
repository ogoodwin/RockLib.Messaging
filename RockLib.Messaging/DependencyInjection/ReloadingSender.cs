﻿#if !NET451
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.DependencyInjection
{
    internal class ReloadingSender<TSenderOptions> : ISender
    {
        public ReloadingSender(string name, Func<TSenderOptions, ISender> createSender, TSenderOptions initialOptions,
            IOptionsMonitor<TSenderOptions> optionsMonitor, Action<TSenderOptions> configureOptions)
        {
            Name = name;
            CreateSender = createSender;
            ConfigureOptions = configureOptions;

            Sender = CreateSender.Invoke(initialOptions);
            ChangeListener = optionsMonitor.OnChange(OptionsMonitorChanged);
        }

        public string Name { get; }

        public ISender Sender { get; private set; }

        public Func<TSenderOptions, ISender> CreateSender { get; }

        public Action<TSenderOptions> ConfigureOptions { get; }

        public IDisposable ChangeListener { get; }

        public Task SendAsync(SenderMessage message, CancellationToken cancellationToken) =>
            Sender.SendAsync(message, cancellationToken);

        public void Dispose()
        {
            ChangeListener.Dispose();
            Sender.Dispose();
        }

        private void OptionsMonitorChanged(TSenderOptions options, string name)
        {
            if (string.Equals(Name, name, StringComparison.OrdinalIgnoreCase))
            {
                ConfigureOptions?.Invoke(options);

                var oldSender = Sender;
                var newSender = CreateSender.Invoke(options);

                Sender = newSender;
                oldSender.Dispose();
            }
        }
    }
}
#endif
