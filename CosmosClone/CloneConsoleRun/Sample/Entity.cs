// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.


namespace CloneConsoleRun.Sample
{
    using System;
    using System.Collections.Generic;
    using CosmosCloneCommon.Model;

    public class Entity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Boolean IsActive { get; set; }
        public DateTime ModifiedTime { get; set; }

        public List<EntityKey> BaseKeys { get; set; }

        public static Entity getRandomEntity()
        {
            var entity = new Entity();
            entity.Id = RandomNumberGenerator.GetNext();
            entity.Name = "Test Sample Name " + entity.Id.ToString();
            entity.Description = "Test Sample Description " + entity.Id.ToString();
            entity.IsActive = true;
            entity.ModifiedTime = DateTime.UtcNow;

            entity.BaseKeys = new List<EntityKey>();
            entity.BaseKeys.Add(EntityKey.getRandomEntityKey());
            entity.BaseKeys.Add(EntityKey.getRandomEntityKey());
            entity.BaseKeys.Add(EntityKey.getRandomEntityKey());
            entity.BaseKeys.Add(EntityKey.getRandomEntityKey());
            entity.BaseKeys.Add(EntityKey.getRandomEntityKey());
            return entity;
        }
    }



    public class CompositeEntity
    {
        public string id { get; set; }
        public string _etag { get; set; }
        public string CompositeName { get; set; }
        public string EmployeeId { get; set; }
        public string EmailAddress { get; set; }
        public string EntityType { get; set; }
        public Entity EntityValue { get; set; }
        public List<EntityKey> SuperKeys { get; set; }

        public static CompositeEntity getRandomCompositeEntity()
        {
            var compositeEntity = new CompositeEntity();
            compositeEntity.id = Guid.NewGuid().ToString();
            compositeEntity.EmployeeId = RandomNumberGenerator.GetNext().ToString();
            compositeEntity.EntityType = RandomNumberGenerator.GetRandomEntityType();
            compositeEntity.CompositeName = "Test Sample CompositeName " + RandomNumberGenerator.GetNext();
            compositeEntity.EmailAddress = "test" + RandomNumberGenerator.GetNext() + "@microsoft.com";
            compositeEntity.EntityValue = Entity.getRandomEntity();

            compositeEntity.SuperKeys = new List<EntityKey>();
            compositeEntity.SuperKeys.Add(EntityKey.getRandomEntityKey());
            compositeEntity.SuperKeys.Add(EntityKey.getRandomEntityKey());
            compositeEntity.SuperKeys.Add(EntityKey.getRandomEntityKey());
            compositeEntity.SuperKeys.Add(EntityKey.getRandomEntityKey());
            compositeEntity.SuperKeys.Add(EntityKey.getRandomEntityKey());
            return compositeEntity;
        }

    }
    public class EntityKey
    {
        public string KeyName { get; set; }
        public string KeyValue { get; set; }
        public string KeyId { get; set; }
        public static EntityKey getRandomEntityKey()
        {
            var entityKey = new EntityKey();
            entityKey.KeyId = RandomNumberGenerator.GetNext().ToString();
            entityKey.KeyName = "TestKeyName-" + entityKey.KeyId.ToString();
            entityKey.KeyValue = "TestKeyValue-" + entityKey.KeyId.ToString();
            return entityKey;
        }
    }

    public class Entitytest
    {
        public Entity EntityValue { get; set; }
        public List<string> Words { get; set; }
        public static Entitytest getRandomTestEntity()
        {
            var testEntity = new Entitytest();
            testEntity.EntityValue = Entity.getRandomEntity();
            testEntity.Words = new List<string>();
            testEntity.Words.Add("test123");
            testEntity.Words.Add("value123");
            testEntity.Words.Add("rex123");
            return testEntity;
        }
           }

    



}
