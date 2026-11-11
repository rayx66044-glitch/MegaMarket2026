using System;
using System.Collections.Generic;

namespace megamarket.Models;

public partial class Role
{
    public int Id { get; set; }

    public string Rolename { get; set; } = null!;

    public int Permissionsmask { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
