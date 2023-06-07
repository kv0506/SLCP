﻿namespace SLCP.ServiceModel;

public class Lock
{
	public Guid Id { get; set; }

	public string Name { get; set; }

	public string Description { get; set; }

	public string Location { get; set; }

	public bool IsEnabled { get; set; }

	public Guid OrganizationId { get; set; }
}