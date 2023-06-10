using SLCP.Core;
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
		await _cosmosService.CreateContainerIfNotExistsAsync(ContainerNames.ApiKey, "/organizationId",
			cancellationToken);
		await _cosmosService.CreateContainerIfNotExistsAsync(ContainerNames.Locations, "/organizationId",
			cancellationToken);
		await _cosmosService.CreateContainerIfNotExistsAsync(ContainerNames.Users, "/organizationId",
			cancellationToken);
		await _cosmosService.CreateContainerIfNotExistsAsync(ContainerNames.UserAccessGroups, "/locationId",
			cancellationToken);
		await _cosmosService.CreateContainerIfNotExistsAsync(ContainerNames.Locks, "/locationId",
			cancellationToken);
		await _cosmosService.CreateContainerIfNotExistsAsync(ContainerNames.LockHubs, "/locationId",
			cancellationToken);
		await _cosmosService.CreateContainerIfNotExistsAsync(ContainerNames.AccessTags, "/locationId",
			cancellationToken);
		await _cosmosService.CreateContainerIfNotExistsAsync(ContainerNames.LockAccessLogs, "/locationId",
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
				org.Id.ToHyphens(), org.Id.ToHyphens(), cancellationToken);

			if (orgServerItem != null)
				return;
		}
		catch (System.Exception e)
		{
			//ignore
		}

		await _cosmosService.UpsertItemAsync(ContainerNames.Organizations,
			org, org.Id.ToHyphens(), cancellationToken);

		var location1 = GetNewLocationObject("Amsterdam Office", org);
		var location2 = GetNewLocationObject("Den Haag Office", org);

		await CreateApiKeys(new List<ApiKey> { GetNewApiKeyObject(location1), GetNewApiKeyObject(location2) });

		var location1Locks = new List<Lock>
		{
			GetNewLockObject("Door1", location1), GetNewLockObject("Door2", location1),
			GetNewLockObject("Door3", location1),
			GetNewLockObject("Door4", location1), GetNewLockObject("Door5", location1)
		};

		var location2Locks = new List<Lock>
		{
			GetNewLockObject("Door1", location2), GetNewLockObject("Door2", location2),
			GetNewLockObject("Door3", location2),
		};

		await CreateLocks(location1Locks);
		await CreateLocks(location2Locks);

		location1.Locks = location1Locks;
		location2.Locks = location2Locks;

		await CreateLocations(new List<Location> { location1, location2 });

		var users = new List<User>
		{
			GetNewUserObject("user1", Roles.Employee, org, location1),
			GetNewUserObject("user2", Roles.Employee, org, location1),
			GetNewUserObject("user3", Roles.SecurityAdmin, org, location1),
			GetNewUserObject("user4", Roles.Employee, org, location2),
			GetNewUserObject("user5", Roles.Employee, org, location2),
			GetNewUserObject("user6", Roles.SecurityAdmin, org, location2),
		};

		var tags = new List<AccessTag>
		{
			GetNewAccessTagObject("123456", location1, users[0]),
			GetNewAccessTagObject("789752", location1, users[1]),
			GetNewAccessTagObject("878952", location2, users[2]),
			GetNewAccessTagObject("657512", location2, users[3]),
			GetNewAccessTagObject("879752", location1, users[4]),
			GetNewAccessTagObject("998822", location2, users[5])
		};
		await CreateTags(tags);

		// set tag Id in user object
		foreach (var user in users)
		{
			var tag = tags.FirstOrDefault(x => x.User.Id == user.Id);
			if (tag != null)
			{
				user.Tags = new List<Guid> { tag.Id };
			}
		}

		await CreateUsers(users);

		var lockHubs = new List<LockHub>
		{
			GetNewLockHubObject(location1, location1Locks),
			GetNewLockHubObject(location2, location2Locks)
		};

		await CreateLockHubs(lockHubs);

		var accessGroups = new List<UserAccessGroup>
		{
			GetNewUserAccessGroup(location1, new List<Lock> { location1Locks[0], location1Locks[1], location1Locks[2] },
				new List<User> { users[0], users[1], users[2] }),
			GetNewUserAccessGroup(location1, new List<Lock> { location1Locks[3], location1Locks[4] },
				new List<User> { users[2] }),
			GetNewUserAccessGroup(location2, new List<Lock> { location2Locks[0], location2Locks[1] },
				new List<User> { users[3], users[4], users[5] }),
			GetNewUserAccessGroup(location2, new List<Lock> { location2Locks[2] },
				new List<User> { users[5] })
		};

		await CreateUserAccessGroups(accessGroups);
	}

	private async Task CreateLocations(IList<Location> locations)
	{
		foreach (var location in locations)
		{
			await _cosmosService.UpsertItemAsync(ContainerNames.Locations, location,
				location.OrganizationId.ToHyphens(), CancellationToken.None);
		}
	}

	private async Task CreateApiKeys(IList<ApiKey> apiKeys)
	{
		foreach (var apiKey in apiKeys)
		{
			await _cosmosService.UpsertItemAsync(ContainerNames.ApiKey, apiKey, apiKey.OrganizationId.ToHyphens(),
				CancellationToken.None);
		}
	}

	private async Task CreateLocks(IList<Lock> locks)
	{
		foreach (var lockObj in locks)
		{
			await _cosmosService.UpsertItemAsync(ContainerNames.Locks,
				lockObj, lockObj.LocationId.ToHyphens(), CancellationToken.None);
		}
	}

	private async Task CreateUsers(IList<User> users)
	{
		foreach (var user in users)
		{
			await _cosmosService.UpsertItemAsync(ContainerNames.Users,
				user, user.OrganizationId.ToHyphens(), CancellationToken.None);
		}
	}

	private async Task CreateTags(IList<AccessTag> tags)
	{
		foreach (var tag in tags)
		{
			await _cosmosService.CreateItemAsync(ContainerNames.AccessTags, tag, tag.LocationId.ToHyphens(),
				CancellationToken.None);
		}
	}

	private async Task CreateLockHubs(IList<LockHub> lockHubs)
	{
		foreach (var lockHub in lockHubs)
		{
			await _cosmosService.CreateItemAsync(ContainerNames.LockHubs, lockHub, lockHub.LocationId.ToHyphens(),
				CancellationToken.None);
		}
	}

	private async Task CreateUserAccessGroups(IList<UserAccessGroup> accessGroups)
	{
		foreach (var accessGroup in accessGroups)
		{
			await _cosmosService.CreateItemAsync(ContainerNames.UserAccessGroups, accessGroup,
				accessGroup.LocationId.ToHyphens(),
				CancellationToken.None);
		}
	}

	private Lock GetNewLockObject(string name, Location location)
	{
		return new Lock
		{
			Id = Guid.NewGuid(),
			Name = name,
			IsEnabled = true,
			LocationId = location.Id,
			OrganizationId = location.OrganizationId
		};
	}

	private User GetNewUserObject(string name, string role, Organization org, Location location)
	{
		var user = new User
		{
			Id = Guid.NewGuid(),
			Name = name,
			EmailAddress = $"{name}@{org.Name}.com",
			Salt = "5691519521109491810",
			PasswordHash = "BPH1G3qA03QhhrFU/kANIf0PEKJP6jUpRSHZhp5Iwo4=",
			LockAccessCodeHash = "uF42wQprBRJdWoacjU+M3+bBSwV+KtgpH8fJPQJwyDg=",
			Role = role,
			OrganizationId = org.Id,
			Locations = new List<Guid> { location.Id },
			Tags = new List<Guid>()
		};

		return user;
	}

	private Location GetNewLocationObject(string name, Organization org)
	{
		var location = new Location
		{
			Id = Guid.NewGuid(),
			Name = name,
			OrganizationId = org.Id
		};

		return location;
	}

	private AccessTag GetNewAccessTagObject(string number, Location location, User user)
	{
		return new AccessTag
		{
			Id = Guid.NewGuid(),
			Number = number,
			LocationId = location.Id,
			OrganizationId = location.OrganizationId,
			User = user
		};
	}

	private ApiKey GetNewApiKeyObject(Location location)
	{
		return new ApiKey
		{
			Id = Guid.NewGuid(),
			Name = $"{location.Name}-ApiKey",
			Key = Guid.NewGuid().ToString("N"),
			LocationId = location.Id,
			OrganizationId = location.OrganizationId
		};
	}

	private LockHub GetNewLockHubObject(Location location, IList<Lock> locks)
	{
		return new LockHub
		{
			Id = Guid.NewGuid(),
			Name = "AMS Lock Hub",
			LocationId = location.Id,
			OrganizationId = location.OrganizationId,
			Locks = locks
		};
	}

	private UserAccessGroup GetNewUserAccessGroup(Location location, IList<Lock> locks, IList<User> users)
	{
		return new UserAccessGroup
		{
			Id = Guid.NewGuid(),
			LocationId = location.Id,
			OrganizationId = location.OrganizationId,
			Locks = locks,
			Users = users
		};
	}
}