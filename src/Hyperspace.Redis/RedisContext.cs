﻿using Hyperspace.Redis.Infrastructure;
using Hyperspace.Redis.Internal;
using Hyperspace.Redis.Properties;
using Hyperspace.Redis.Storage;
using JetBrains.Annotations;
using Microsoft.Framework.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;

namespace Hyperspace.Redis
{
    public class RedisContext : IServiceProvider
    {
        private static readonly ConcurrentDictionary<Type, Type> OptionsTypes = new ConcurrentDictionary<Type, Type>();
        private RedisDatabase _database;

        protected RedisContext()
        {
            var serviceProvider = RedisContextActivator.ServiceProvider;
            Initialize(serviceProvider, GetOptions(serviceProvider));
        }

        public RedisContext([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));

            Initialize(serviceProvider, GetOptions(serviceProvider));
        }

        public RedisContext([NotNull] RedisContextOptions options)
        {
            Check.NotNull(options, nameof(options));

            Initialize(RedisContextActivator.ServiceProvider, options);
        }

        public RedisContext([NotNull] IServiceProvider serviceProvider, [NotNull] RedisContextOptions options)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));
            Check.NotNull(options, nameof(options));

            Initialize(serviceProvider, options);
        }

        private RedisContextOptions GetOptions(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                return new RedisContextOptions<RedisContext>();

            var genericOptions = OptionsTypes.GetOrAdd(GetType(), t => typeof(RedisContextOptions<>).MakeGenericType(t));
            var options = (RedisContextOptions)serviceProvider.GetService(genericOptions) ?? serviceProvider.GetService<RedisContextOptions>();

            if (options != null && options.GetType() != genericOptions)
                throw new InvalidOperationException(Strings.NonGenericOptions);

            return options ?? new RedisContextOptions<RedisContext>();
        }

        private void Initialize(IServiceProvider serviceProvider, RedisContextOptions options)
        {
            var connectionProvider = serviceProvider.GetRequiredService<IRedisConnectionProvider>();
            _database = (RedisDatabase)connectionProvider.ConnectAndSelect(options);
        }

        public IDatabase Database { get { return _database.Database; } }

        #region IServiceProvider

        object IServiceProvider.GetService(Type serviceType)
        {
            return serviceType;
        }

        #endregion

    }
}