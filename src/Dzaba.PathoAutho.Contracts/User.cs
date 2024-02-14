﻿using System;

namespace Dzaba.PathoAutho.Contracts;

public sealed class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
