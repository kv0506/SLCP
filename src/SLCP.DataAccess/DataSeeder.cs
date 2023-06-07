using CSharpExtensions;
using SLCP.DataAccess.CosmosService;
using SLCP.ServiceModel;

namespace SLCP.DataAccess;

public interface IDataSeeder
{
	void SeedData(CancellationToken cancellationToken);
}

public class DataSeeder : IDataSeeder
{
	private readonly ICosmosService _cosmosService;

	public DataSeeder(ICosmosService cosmosService)
	{
		_cosmosService = cosmosService;
	}

	public async void SeedData(CancellationToken cancellationToken)
	{
		await CreateContainers(cancellationToken);
		await SetupOrganization(cancellationToken);
	}

	private async Task CreateContainers(CancellationToken cancellationToken)
	{
		await _cosmosService.CreateContainerIfNotExistsAsync(ContainerNames.Organizations, "/id", cancellationToken);
		await _cosmosService.CreateContainerIfNotExistsAsync(ContainerNames.Users, "/organizationId",
			cancellationToken);
		await _cosmosService.CreateContainerIfNotExistsAsync(ContainerNames.Locks, "/organizationId",
			cancellationToken);
		await _cosmosService.CreateContainerIfNotExistsAsync(ContainerNames.AccessTags, "/organizationId",
			cancellationToken);
		await _cosmosService.CreateContainerIfNotExistsAsync(ContainerNames.LockGroups, "/organizationId",
			cancellationToken);
		await _cosmosService.CreateContainerIfNotExistsAsync(ContainerNames.LockAccessLogs, "/organizationId",
			cancellationToken);
	}

	private async Task SetupOrganization(CancellationToken cancellationToken)
	{
		var org = new Organization
		{
			Id = Guid.Parse("c590db46-2338-44da-8093-08b8b08ee2b6"),
			Address = "Amsterdam",
			EmailAddress = "abc@test.com",
			Name = "ABC"
		};

		try
		{
			var orgServerItem = await _cosmosService.GetItemAsync<Organization>(ContainerNames.Organizations,
				"c590db46-2338-44da-8093-08b8b08ee2b6", "c590db46-2338-44da-8093-08b8b08ee2b6", cancellationToken);

			if(orgServerItem!=null)
				return;
		}
		catch (Exception e)
		{
			//ignore
		}

		await _cosmosService.UpsertItemAsync(ContainerNames.Organizations,
			org, org.Id.ToHyphens(), cancellationToken);

		var lock1 = CreateNewLock("Door1", org);
		var lock2 = CreateNewLock("Door2", org);
		var lock3 = CreateNewLock("Door3", org);
		var lock4 = CreateNewLock("Door4", org);
		var lock5 = CreateNewLock("Door5", org);

		await _cosmosService.UpsertItemAsync(ContainerNames.Locks,
			lock1, org.Id.ToHyphens(), cancellationToken);
		await _cosmosService.UpsertItemAsync(ContainerNames.Locks,
			lock2, org.Id.ToHyphens(), cancellationToken);
		await _cosmosService.UpsertItemAsync(ContainerNames.Locks,
			lock3, org.Id.ToHyphens(), cancellationToken);
		await _cosmosService.UpsertItemAsync(ContainerNames.Locks,
			lock4, org.Id.ToHyphens(), cancellationToken);
		await _cosmosService.UpsertItemAsync(ContainerNames.Locks,
			lock5, org.Id.ToHyphens(), cancellationToken);

		var lockGroup1 = CreateNewLockGroup("EntryDoors", org, lock1, lock2, lock3);
		var lockGroup2 = CreateNewLockGroup("ServerRooms", org, lock4, lock5);

		await _cosmosService.UpsertItemAsync(ContainerNames.LockGroups,
			lockGroup1, org.Id.ToHyphens(), cancellationToken);
		await _cosmosService.UpsertItemAsync(ContainerNames.LockGroups,
			lockGroup2, org.Id.ToHyphens(), cancellationToken);

		var user1 = CreateNewUser("TestUser1", org, lockGroup1);
		var user2 = CreateNewUser("TestUser2", org, lockGroup1);
		var user3 = CreateNewUser("TestUser3", org, lockGroup1, lockGroup2);

		await _cosmosService.UpsertItemAsync(ContainerNames.Users,
			user1, org.Id.ToHyphens(), cancellationToken);
		await _cosmosService.UpsertItemAsync(ContainerNames.Users,
			user2, org.Id.ToHyphens(), cancellationToken);
		await _cosmosService.UpsertItemAsync(ContainerNames.Users,
			user3, org.Id.ToHyphens(), cancellationToken);

		var user1Tag = CreateNewAccessTag("12345", org, user1);
		var user2Tag = CreateNewAccessTag("56897", org, user2);
		var user3Tag = CreateNewAccessTag("87987", org, user3);

		await _cosmosService.UpsertItemAsync(ContainerNames.AccessTags,
			user1Tag, org.Id.ToHyphens(), cancellationToken);
		await _cosmosService.UpsertItemAsync(ContainerNames.AccessTags,
			user2Tag, org.Id.ToHyphens(), cancellationToken);
		await _cosmosService.UpsertItemAsync(ContainerNames.AccessTags,
			user3Tag, org.Id.ToHyphens(), cancellationToken);
	}

	private Lock CreateNewLock(string name, Organization org)
	{
		return new Lock
		{
			Id = Guid.NewGuid(),
			Name = name,
			IsEnabled = true,
			OrganizationId = org.Id
		};
	}

	private User CreateNewUser(string name, Organization org, params LockGroup[] lockGroups)
	{
		var user = new User
		{
			Id = Guid.NewGuid(),
			Name = name,
			EmailAddress = $"{name}@{org.Name}.com",
			PasswordSalt = "5691519521109491810",
			PasswordHash = "BPH1G3qA03QhhrFU/kANIf0PEKJP6jUpRSHZhp5Iwo4=",
			LockAccessCode = "12345678",
			OrganizationId = org.Id,
			PermittedLockGroups = new List<LockGroup>()
		};

		if (lockGroups != null)
		{
			user.PermittedLockGroups.AddRange(lockGroups);
		}

		return user;
	}

	private LockGroup CreateNewLockGroup(string name, Organization org, params Lock[] locks)
	{
		var lockObj =new LockGroup
		{
			Id = Guid.NewGuid(),
			Name = name,
			Locks = new List<Lock>(),
			OrganizationId = org.Id
		};

		if (locks != null)
		{
			lockObj.Locks.AddRange(locks);
		}

		return lockObj;
	}

	private AccessTag CreateNewAccessTag(string number, Organization org, User user)
	{
		return new AccessTag
		{
			Id = Guid.NewGuid(),
			Number = number,
			OrganizationId = org.Id,
			User = user
		};
	}
}