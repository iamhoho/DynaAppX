using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynaAppX.Services
{
    public class SharedMetadataCache
    {
        private static SharedMetadataCache _instance;
        private static readonly object _lock = new object();

        private readonly Dictionary<IOrganizationService, ServiceCache> _serviceCaches = new Dictionary<IOrganizationService, ServiceCache>();
        private IOrganizationService _currentService;

        private SharedMetadataCache() { }

        public static SharedMetadataCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SharedMetadataCache();
                        }
                    }
                }
                return _instance;
            }
        }

        public void Initialize(IOrganizationService service)
        {
            _currentService = service;
            // Don't auto-load entities - only create cache entry, load on demand
        }

        public void Clear()
        {
            lock (_lock)
            {
                _serviceCaches.Clear();
                _currentService = null;
            }
        }

        public void ClearForService(IOrganizationService service)
        {
            lock (_lock)
            {
                if (_serviceCaches.ContainsKey(service))
                {
                    _serviceCaches.Remove(service);
                }
            }
        }

        private ServiceCache GetOrCreateCache(IOrganizationService service)
        {
            lock (_lock)
            {
                if (!_serviceCaches.TryGetValue(service, out var cache))
                {
                    cache = new ServiceCache();
                    _serviceCaches[service] = cache;
                }
                return cache;
            }
        }

        public List<EntityWrapper> GetAllEntities(IOrganizationService service)
        {
            var cache = GetOrCreateCache(service);
            return cache.Entities ?? new List<EntityWrapper>();
        }

        public List<UserWrapper> GetRecentUsers(IOrganizationService service)
        {
            var cache = GetOrCreateCache(service);
            return cache.RecentUsers ?? new List<UserWrapper>();
        }

        public void RefreshEntities(IOrganizationService service)
        {
            var cache = GetOrCreateCache(service);

            var request = new RetrieveAllEntitiesRequest
            {
                EntityFilters = EntityFilters.Entity | EntityFilters.Attributes,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveAllEntitiesResponse)service.Execute(request);

            var entities = new List<EntityWrapper>();
            var entitiesByMetadataId = new Dictionary<Guid, EntityWrapper>();

            foreach (var entity in response.EntityMetadata)
            {
                if (entity.IsIntersect == true) continue;
                if (string.IsNullOrEmpty(entity.LogicalName)) continue;

                var wrapper = new EntityWrapper
                {
                    LogicalName = entity.LogicalName,
                    DisplayName = entity.DisplayName?.UserLocalizedLabel?.Label ?? entity.LogicalName,
                    EntitySetName = entity.EntitySetName,
                    PrimaryIdAttribute = entity.PrimaryIdAttribute,
                    PrimaryNameAttribute = entity.PrimaryNameAttribute,
                    MetadataId = entity.MetadataId ?? Guid.Empty,
                    Attributes = entity.Attributes?.ToList() ?? new List<AttributeMetadata>()
                };

                entities.Add(wrapper);
                if (wrapper.MetadataId != Guid.Empty)
                {
                    entitiesByMetadataId[wrapper.MetadataId] = wrapper;
                }
            }

            lock (_lock)
            {
                cache.Entities = entities;
                cache.EntitiesByMetadataId = entitiesByMetadataId;
                cache.LastRefresh = DateTime.Now;
            }
        }

        public async Task<List<UserWrapper>> SearchUsersAsync(IOrganizationService service, string searchText)
        {
            var escapedSearch = EscapeXml(searchText ?? "");
            var fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='30'>
                <entity name='systemuser'>
                    <attribute name='systemuserid'/>
                    <attribute name='fullname'/>
                    <attribute name='domainname'/>
                    <order attribute='fullname' descending='false'/>
                    <filter type='and'>
                        <condition attribute='isdisabled' operator='eq' value='0'/>
                        {(!string.IsNullOrEmpty(escapedSearch) ? $"<condition attribute='fullname' operator='like' value='%{escapedSearch}%'/>" : "")}
                    </filter>
                </entity>
            </fetch>";

            var result = await Task.Run(() => service.RetrieveMultiple(new FetchExpression(fetchXml)));
            var users = result.Entities.Select(u => new UserWrapper
            {
                Id = u.Id,
                FullName = u.GetAttributeValue<string>("fullname") ?? "(No name)",
                DomainName = u.GetAttributeValue<string>("domainname"),
                Entity = u
            }).ToList();

            var cache = GetOrCreateCache(service);
            cache.RecentUsers = users;

            return users;
        }

        private string EscapeXml(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
        }

        public List<EntityWrapper> FilterEntities(IOrganizationService service, string searchText)
        {
            var allEntities = GetAllEntities(service);
            if (string.IsNullOrEmpty(searchText))
                return allEntities;

            var searchLower = searchText.ToLowerInvariant();
            return allEntities.Where(e =>
                e.LogicalName.ToLowerInvariant().Contains(searchLower) ||
                e.DisplayName.ToLowerInvariant().Contains(searchLower)
            ).ToList();
        }

        public EntityWrapper GetEntityByMetadataId(IOrganizationService service, Guid metadataId)
        {
            var cache = GetOrCreateCache(service);
            return cache.EntitiesByMetadataId.TryGetValue(metadataId, out var entity) ? entity : null;
        }

        private class ServiceCache
        {
            public List<EntityWrapper> Entities { get; set; }
            public Dictionary<Guid, EntityWrapper> EntitiesByMetadataId { get; set; }
            public List<UserWrapper> RecentUsers { get; set; }
            public List<FlowWrapper> Flows { get; set; }
            public DateTime LastRefresh { get; set; }
        }

        public List<FlowWrapper> GetFlows(IOrganizationService service)
        {
            var cache = GetOrCreateCache(service);
            return cache.Flows ?? new List<FlowWrapper>();
        }

        public void SetFlows(IOrganizationService service, List<FlowWrapper> flows)
        {
            var cache = GetOrCreateCache(service);
            cache.Flows = flows;
        }
    }

    public class EntityWrapper
    {
        public Guid MetadataId { get; set; }
        public string LogicalName { get; set; }
        public string DisplayName { get; set; }
        public string EntityDisplayName => DisplayName;
        public string EntitySetName { get; set; }
        public string PrimaryIdAttribute { get; set; }
        public string PrimaryNameAttribute { get; set; }
        public List<AttributeMetadata> Attributes { get; set; }
        public Entity Entity { get; set; }
    }

    public class UserWrapper
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string DomainName { get; set; }
        public string DisplayName => FullName;
        public Entity Entity { get; set; }
    }

    public class FlowWrapper
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string UniqueName { get; set; }
        public int Category { get; set; }
        public string CategoryName { get; set; }
        public string PrimaryEntity { get; set; }
        public string Xaml { get; set; }
    }
}