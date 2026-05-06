using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;

namespace DynaAppX.WpfControls
{
    /// <summary>
    /// Static cache shared across all WPF control instances for entity metadata and flows.
    /// Keyed by CRM service instance. Cleared on connection change.
    /// </summary>
    public static class SharedMetadataCache
    {
        private static IOrganizationService _service;
        private static readonly object _lock = new object();

        // Entity metadata cache
        private static List<EntityWrapper> _entities;
        private static Dictionary<string, EntityMetadata> _entityMetadataCache;

        // Flow cache
        private static List<FlowWrapper> _flows;

        public static IOrganizationService Service
        {
            get => _service;
        }

        /// <summary>
        /// Call this when CRM connection changes (in MyPluginControl.UpdateConnection)
        /// to invalidate all cached data.
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                _service = null;
                _entities = null;
                _entityMetadataCache = null;
                _flows = null;
            }
        }

        /// <summary>
        /// Call this to set the current service. If service changed from previous,
        /// cache is automatically invalidated.
        /// </summary>
        public static void SetService(IOrganizationService service)
        {
            lock (_lock)
            {
                if (_service != service)
                {
                    Clear();
                    _service = service;
                }
            }
        }

        // === Entity metadata ===

        public static List<EntityWrapper> GetEntities(IOrganizationService service)
        {
            lock (_lock)
            {
                if (_service != service) return null;
                return _entities;
            }
        }

        public static void SetEntities(IOrganizationService service, List<EntityWrapper> entities)
        {
            lock (_lock)
            {
                if (_service != service) return;
                _entities = entities;
            }
        }

        public static Dictionary<string, EntityMetadata> GetMetadataCache(IOrganizationService service)
        {
            lock (_lock)
            {
                if (_service != service) return null;
                return _entityMetadataCache;
            }
        }

        public static void SetMetadataCache(IOrganizationService service, Dictionary<string, EntityMetadata> cache)
        {
            lock (_lock)
            {
                if (_service != service) return;
                _entityMetadataCache = cache;
            }
        }

        // === Flows ===

        public static List<FlowWrapper> GetFlows(IOrganizationService service)
        {
            lock (_lock)
            {
                if (_service != service) return null;
                return _flows;
            }
        }

        public static void SetFlows(IOrganizationService service, List<FlowWrapper> flows)
        {
            lock (_lock)
            {
                if (_service != service) return;
                _flows = flows;
            }
        }
    }
}
