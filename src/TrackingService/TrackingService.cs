namespace TrackingService
{
    using System;
    using System.Configuration;
    using System.Threading.Tasks;
    using Automatonymous;
    using CartTracking;
    using MassTransit;
    using MassTransit.EntityFrameworkIntegration;
    using MassTransit.EntityFrameworkIntegration.Saga;
    using MassTransit.QuartzIntegration;
    using MassTransit.RabbitMqTransport;
    using MassTransit.Saga;
    using Quartz;
    using Quartz.Impl;
    using Topshelf;
    using Topshelf.Logging;


    class TrackingService :
        ServiceControl
    {
        readonly LogWriter _log = HostLogger.Get<TrackingService>();
        readonly IScheduler _scheduler;

        IBusControl _busControl;
        BusHandle _busHandle;
        ShoppingCartStateMachine _machine;
        Lazy<ISagaRepository<ShoppingCart>> _repository;

        public TrackingService()
        {
            _scheduler = CreateScheduler();
        }

        public bool Start(HostControl hostControl)
        {
            _log.Info("Creating bus...");

            _machine = new ShoppingCartStateMachine();

            SagaDbContextFactory sagaDbContextFactory =
                () => new SagaDbContext<ShoppingCart, ShoppingCartMap>(SagaDbContextFactoryProvider.ConnectionString);

            _repository = new Lazy<ISagaRepository<ShoppingCart>>(
                () => new EntityFrameworkSagaRepository<ShoppingCart>(sagaDbContextFactory));

            _busControl = Bus.Factory.CreateUsingRabbitMq(x =>
            {
                IRabbitMqHost host = x.Host(new Uri(ConfigurationManager.AppSettings["RabbitMQHost"]), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                x.ReceiveEndpoint(host, "shopping_cart_state", e =>
                {
                    e.PrefetchCount = 8;
                    e.StateMachineSaga(_machine, _repository.Value);
                });

                x.ReceiveEndpoint(host, ConfigurationManager.AppSettings["SchedulerQueueName"], e =>
                {
					// For MT4.0, prefetch must be set for Quartz prior to anything else
                    e.PrefetchCount = 1;
                    x.UseMessageScheduler(e.InputAddress);                   

                    e.Consumer(() => new ScheduleMessageConsumer(_scheduler));
                    e.Consumer(() => new CancelScheduledMessageConsumer(_scheduler));
                });
            });

            _log.Info("Starting bus...");

            try
            {
                _busHandle = MassTransit.Util.TaskUtil.Await<BusHandle>(()=>_busControl.StartAsync());

                _scheduler.JobFactory = new MassTransitJobFactory(_busControl);

                _scheduler.Start();
            }
            catch (Exception)
            {
                _scheduler.Shutdown();
                throw;
            }

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _log.Info("Stopping bus...");

            _scheduler.Standby();

            if (_busHandle != null)
                _busHandle.Stop();

            _scheduler.Shutdown();

            return true;
        }


        static IScheduler CreateScheduler()
        {
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();            
            IScheduler scheduler = MassTransit.Util.TaskUtil.Await<IScheduler>(() => schedulerFactory.GetScheduler()); ;

            return scheduler;
        }
    }
}