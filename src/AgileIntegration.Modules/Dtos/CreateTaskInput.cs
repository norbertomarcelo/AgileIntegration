﻿namespace AgileIntegration.Modules.Dtos;

public class CreateTaskInput
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int Priority { get; set; }
    public int Status { get; set; }
}
