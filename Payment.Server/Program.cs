using Payment.Server;

await new PaymentServerStartup().RunHostAsync(new CancellationToken());
