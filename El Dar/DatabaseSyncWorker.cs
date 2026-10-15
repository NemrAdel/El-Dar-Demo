using El_Dar.Models;
using ElDarDal.Context;
using ElDarDal.Models;
using ElDarDal.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;

namespace El_Dar
{
    public class DatabaseSyncWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Supabase.Client _supabaseClient;
        private readonly string? _monsterConnectionString;
        private readonly TimeSpan _syncInterval = TimeSpan.FromSeconds(5);

        private DateTime _lastGuestsSync = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private DateTime _lastEmployeesSync = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private DateTime _lastDataSync = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private DateTime _lastUserSync = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private DateTime _lastDeleteSync = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public DatabaseSyncWorker(IServiceProvider serviceProvider, Supabase.Client supabaseClient , IConfiguration configuration )
        {
            _serviceProvider = serviceProvider;
            _supabaseClient = supabaseClient;
            _monsterConnectionString = configuration.GetConnectionString("Monsterconnection");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ElDarDbContext>();

                        try { await SyncGuestsUpsertsAsync(dbContext); }
                        catch (Exception ex) { Console.WriteLine($"[Guests Sync Error]: {ex.Message}"); }

                        try { await SyncEmployeesUpsertsAsync(dbContext); }
                        catch (Exception ex) { Console.WriteLine($"[Employees Sync Error]: {ex.Message}"); }

                        try { await SyncDataUpsertsAsync(dbContext); }
                        catch (Exception ex) { Console.WriteLine($"[Data Sync Error]: {ex.Message}"); }

                        try { await SyncUserUpsertsAsync(dbContext); }
                        catch (Exception ex) { Console.WriteLine($"[User Sync Error]: {ex.Message}"); }

                        try { await DeletedLogs(dbContext); }
                        catch (Exception ex) { Console.WriteLine($"[DeletedLogs Sync Error]: {ex.Message}"); }
                    }
                }

                await Task.Delay(_syncInterval, stoppingToken);
            }
        }

        #region Guests Sync
        private async Task SyncGuestsUpsertsAsync(ElDarDbContext dbContext)
        {
            var unsyncedGuests = await dbContext.Guests.Where(g => !g.IsSynced).ToListAsync();
            var justPushedGuestIds = new HashSet<Guid>();
            if (unsyncedGuests.Any())
            {
                var supabaseGuests = unsyncedGuests.Select(g => new SupabaseGuest
                {
                    Id = g.Id,
                    Name = g.Name,
                    PhoneNumber = g.PhoneNumber,
                    CardNumber = g.CardNumber,
                    Address = g.Address,
                    DateOfBirth = DateTime.SpecifyKind(g.DateOfBirth.Date, DateTimeKind.Utc),
                    Price = g.Price,
                    OutPrice = g.OutPrice,
                    Status = g.Status.ToString(),
                    IsPaid = g.IsPaid,
                    Notes = g.Notes,
                    CreatedAt = DateTime.SpecifyKind(g.CreatedAt, DateTimeKind.Utc),
                    UpdatedAt = g.UpdatedAt.HasValue
                        ? DateTime.SpecifyKind(g.UpdatedAt.Value, DateTimeKind.Utc)
                        : DateTime.SpecifyKind(g.CreatedAt, DateTimeKind.Utc),
                    IsDeleted = g.IsDeleted,
                    IsSynced = false
                }).ToList();

                await _supabaseClient.From<SupabaseGuest>().Upsert(supabaseGuests);

                justPushedGuestIds = unsyncedGuests.Select(g => g.Id).ToHashSet();

                foreach (var guest in unsyncedGuests)
                {
                    guest.IsSynced = true;
                }
                await dbContext.SaveChangesAsync();
            }

            var fetchTime = DateTime.UtcNow;

            var supabaseChanges = await _supabaseClient.From<SupabaseGuest>()
                .Where(g => g.IsSynced == false || g.UpdatedAt > _lastGuestsSync)
                .Get();

            var remoteGuests = supabaseChanges.Models
                .Where(rg => !justPushedGuestIds.Contains(rg.Id))
                .ToList();

            if (remoteGuests.Any())
            {
                foreach (var remoteGuest in remoteGuests)
                {
                    var localGuest = await dbContext.Guests.FirstOrDefaultAsync(g => g.Id == remoteGuest.Id);

                    var rawRemoteUpdated = remoteGuest.UpdatedAt ?? remoteGuest.CreatedAt ?? DateTime.UtcNow;
                    var remoteUpdatedAt = DateTime.SpecifyKind(rawRemoteUpdated, DateTimeKind.Utc);

                    DateTime? localUpdatedAt = localGuest?.UpdatedAt.HasValue == true
                        ? DateTime.SpecifyKind(localGuest.UpdatedAt.Value, DateTimeKind.Utc)
                        : null;

                    Status? parsedStatus = Enum.TryParse<Status>(remoteGuest.Status, true, out var st) ? st : null;

                    if (localGuest == null)
                    {
                        dbContext.Guests.Add(new Guest
                        {
                            Id = remoteGuest.Id,
                            Name = remoteGuest.Name,
                            PhoneNumber = remoteGuest.PhoneNumber,
                            CardNumber = remoteGuest.CardNumber,
                            Address = remoteGuest.Address,
                            DateOfBirth = remoteGuest.DateOfBirth.Date,
                            Price = remoteGuest.Price,
                            OutPrice = remoteGuest.OutPrice,
                            Status = parsedStatus ?? default(Status),
                            IsPaid = remoteGuest.IsPaid,
                            Notes = remoteGuest.Notes,
                            CreatedAt = DateTime.SpecifyKind(remoteGuest.CreatedAt ?? DateTime.UtcNow, DateTimeKind.Utc),
                            UpdatedAt = remoteUpdatedAt,
                            IsDeleted = remoteGuest.IsDeleted,
                            IsSynced = true
                        });
                    }
                    else if (localUpdatedAt == null || remoteUpdatedAt > localUpdatedAt)
                    {
                        localGuest.Name = remoteGuest.Name;
                        localGuest.PhoneNumber = remoteGuest.PhoneNumber;
                        localGuest.CardNumber = remoteGuest.CardNumber;
                        localGuest.Address = remoteGuest.Address;
                        localGuest.DateOfBirth = remoteGuest.DateOfBirth.Date;
                        localGuest.Price = remoteGuest.Price;
                        localGuest.OutPrice = remoteGuest.OutPrice;
                        localGuest.Status = parsedStatus ?? localGuest.Status;
                        localGuest.IsPaid = remoteGuest.IsPaid;
                        localGuest.Notes = remoteGuest.Notes;
                        localGuest.UpdatedAt = remoteUpdatedAt;
                        localGuest.IsDeleted = remoteGuest.IsDeleted;
                        localGuest.IsSynced = true;
                    }
                    remoteGuest.IsSynced = true;
                }

                await dbContext.SaveChangesAsync();
                await _supabaseClient.From<SupabaseGuest>().Upsert(remoteGuests);
            }

            _lastGuestsSync = fetchTime;
        }
        #endregion

        #region Employees Sync
        private async Task SyncEmployeesUpsertsAsync(ElDarDbContext dbContext)
        {
            var unsyncedEmployees = await dbContext.Employees.Where(e => !e.IsSynced).ToListAsync();
            var justPushedEmployeeIds = new HashSet<Guid>();
            if (unsyncedEmployees.Any())
            {
                var supabaseEmployees = unsyncedEmployees.Select(emp => new SupabaseEmployee
                {
                    Id = emp.Id,
                    CardNumber = emp.CardNumber,
                    Name = emp.Name,
                    PhoneNumber = emp.PhoneNumber,
                    Address = emp.Address!,
                    DateOfBirth = DateTime.SpecifyKind(emp.DateOfBirth.Date, DateTimeKind.Utc),
                    Salary = emp.Salary,
                    Adds = emp.Adds,
                    Taxs = emp.Taxs,
                    Total = emp.Total,
                    Job = emp.Job.ToString(),
                    Status = emp.Status.ToString(),
                    IsPaid = emp.IsPaid,
                    IsDeleted = emp.IsDeleted,
                    IsSynced = false,
                    CreatedAt = DateTime.SpecifyKind(emp.CreatedAt, DateTimeKind.Utc),
                    UpdatedAt = emp.UpdatedAt.HasValue
                        ? DateTime.SpecifyKind(emp.UpdatedAt.Value, DateTimeKind.Utc)
                        : DateTime.SpecifyKind(emp.CreatedAt, DateTimeKind.Utc),
                    Notes = emp.Notes
                }).ToList();

                await _supabaseClient.From<SupabaseEmployee>().Upsert(supabaseEmployees);

                justPushedEmployeeIds = unsyncedEmployees.Select(e => e.Id).ToHashSet();

                foreach (var employee in unsyncedEmployees)
                {
                    employee.IsSynced = true;
                }
                await dbContext.SaveChangesAsync();
            }

            var fetchTime = DateTime.UtcNow;
            var supabaseChanges = await _supabaseClient.From<SupabaseEmployee>()
                .Where(g => g.IsSynced == false || g.UpdatedAt > _lastEmployeesSync)
                .Get();

            var remoteEmployees = supabaseChanges.Models
                .Where(re => !justPushedEmployeeIds.Contains(re.Id))
                .ToList();

            if (remoteEmployees.Any())
            {
                foreach (var remoteEmployee in remoteEmployees)
                {
                    var localEmployee = await dbContext.Employees.FirstOrDefaultAsync(g => g.Id == remoteEmployee.Id);
                    var rawRemoteEmployeeUpdated = remoteEmployee.UpdatedAt ?? remoteEmployee.CreatedAt ?? DateTime.UtcNow;
                    var remoteUpdatedAt = DateTime.SpecifyKind(rawRemoteEmployeeUpdated, DateTimeKind.Utc);

                    DateTime? localUpdatedAt = localEmployee?.UpdatedAt.HasValue == true
                        ? DateTime.SpecifyKind(localEmployee.UpdatedAt.Value, DateTimeKind.Utc)
                        : null;

                    JobEnum? parsedJob = Enum.TryParse<JobEnum>(remoteEmployee.Job, true, out var job) ? job : null;
                    Status? parsedStatus = Enum.TryParse<Status>(remoteEmployee.Status, true, out var status) ? status : null;

                    if (localEmployee == null)
                    {
                        dbContext.Employees.Add(new Employee
                        {
                            Id = remoteEmployee.Id,
                            CardNumber = remoteEmployee.CardNumber,
                            Name = remoteEmployee.Name,
                            PhoneNumber = remoteEmployee.PhoneNumber,
                            Address = remoteEmployee.Address!,
                            DateOfBirth = remoteEmployee.DateOfBirth.Date,
                            Salary = remoteEmployee.Salary,
                            Adds = remoteEmployee.Adds,
                            Taxs = remoteEmployee.Taxs,
                            Job = parsedJob ?? default(JobEnum),
                            Status = parsedStatus ?? default(Status),
                            IsPaid = remoteEmployee.IsPaid,
                            CreatedAt = DateTime.SpecifyKind(remoteEmployee.CreatedAt ?? DateTime.UtcNow, DateTimeKind.Utc),
                            UpdatedAt = remoteUpdatedAt,
                            IsDeleted = remoteEmployee.IsDeleted,
                            IsSynced = true,
                            Notes = remoteEmployee.Notes
                        });
                    }
                    else if (localUpdatedAt == null || remoteUpdatedAt > localUpdatedAt)
                    {
                        localEmployee.CardNumber = remoteEmployee.CardNumber;
                        localEmployee.Name = remoteEmployee.Name;
                        localEmployee.PhoneNumber = remoteEmployee.PhoneNumber;
                        localEmployee.Address = remoteEmployee.Address!;
                        localEmployee.DateOfBirth = remoteEmployee.DateOfBirth.Date;
                        localEmployee.Salary = remoteEmployee.Salary;
                        localEmployee.Adds = remoteEmployee.Adds;
                        localEmployee.Taxs = remoteEmployee.Taxs;
                        localEmployee.Job = parsedJob ?? localEmployee.Job;
                        localEmployee.Status = parsedStatus ?? localEmployee.Status;
                        localEmployee.IsPaid = remoteEmployee.IsPaid;
                        localEmployee.UpdatedAt = remoteUpdatedAt;
                        localEmployee.IsDeleted = remoteEmployee.IsDeleted;
                        localEmployee.IsSynced = true;
                        localEmployee.Notes = remoteEmployee.Notes;
                    }

                    remoteEmployee.IsSynced = true;
                }

                await dbContext.SaveChangesAsync();
                await _supabaseClient.From<SupabaseEmployee>().Upsert(remoteEmployees);
            }

            _lastEmployeesSync = fetchTime;
        }
        #endregion

        #region Data Sync
        private async Task SyncDataUpsertsAsync(ElDarDbContext dbContext)
        {
            var unsyncedData = await dbContext.Data.Where(d => !d.IsSynced).ToListAsync();
            var justPushedDataIds = new HashSet<Guid>();

            if (unsyncedData.Any())
            {
                var supabaseData = unsyncedData.Select(data => new SupabaseData
                {
                    Id = data.Id,
                    Name = data.Name,
                    Revenues = data.Revenues?.ToString(),
                    Payments = data.Payments?.ToString(),
                    Price = data.Price,
                    OutPrice = data.OutPrice,
                    Payment = data.Payment,
                    Date = DateTime.SpecifyKind(data.Date, DateTimeKind.Utc),
                    CreatedAt = DateTime.SpecifyKind(data.CreatedAt, DateTimeKind.Utc),
                    UpdatedAt = data.UpdatedAt.HasValue
                        ? DateTime.SpecifyKind(data.UpdatedAt.Value, DateTimeKind.Utc)
                        : DateTime.SpecifyKind(data.CreatedAt, DateTimeKind.Utc),
                    IsDeleted = data.IsDeleted,
                    IsSynced = false,
                    Notes = data.Notes
                }).ToList();

                await _supabaseClient.From<SupabaseData>().Upsert(supabaseData);

                justPushedDataIds = unsyncedData.Select(d => d.Id).ToHashSet();

                foreach (var data in unsyncedData)
                {
                    data.IsSynced = true;
                }
                await dbContext.SaveChangesAsync();
            }

            var fetchTime = DateTime.UtcNow;
            var supabaseChanges = await _supabaseClient.From<SupabaseData>()
                .Where(g => g.IsSynced == false || g.UpdatedAt > _lastDataSync)
                .Get();

            var remoteData = supabaseChanges.Models
                .Where(rd => !justPushedDataIds.Contains(rd.Id))
                .ToList();

            if (remoteData.Any())
            {
                foreach (var remoteDataItem in remoteData)
                {
                    var localData = await dbContext.Data.FirstOrDefaultAsync(g => g.Id == remoteDataItem.Id);

                    var rawRemoteDataUpdated = remoteDataItem.UpdatedAt ?? remoteDataItem.CreatedAt ?? DateTime.UtcNow;
                    var remoteUpdatedAt = DateTime.SpecifyKind(rawRemoteDataUpdated, DateTimeKind.Utc);

                    DateTime? localUpdatedAt = localData?.UpdatedAt.HasValue == true
                        ? DateTime.SpecifyKind(localData.UpdatedAt.Value, DateTimeKind.Utc)
                        : null;

                    Revenues? parsedRevenues = Enum.TryParse<Revenues>(remoteDataItem.Revenues, true, out var rev) ? rev : null;
                    Payments? parsedPayments = Enum.TryParse<Payments>(remoteDataItem.Payments, true, out var pay) ? pay : null;

                    if (localData == null)
                    {
                        dbContext.Data.Add(new Data
                        {
                            Id = remoteDataItem.Id,
                            Name = remoteDataItem.Name,
                            Revenues = parsedRevenues,
                            Payments = parsedPayments,
                            Price = remoteDataItem.Price,
                            OutPrice = remoteDataItem.OutPrice,
                            Payment = remoteDataItem.Payment,
                            Date = DateTime.SpecifyKind(remoteDataItem.Date, DateTimeKind.Utc),
                            IsDeleted = remoteDataItem.IsDeleted,
                            CreatedAt = DateTime.SpecifyKind(remoteDataItem.CreatedAt ?? DateTime.UtcNow, DateTimeKind.Utc),
                            UpdatedAt = remoteUpdatedAt,
                            IsSynced = true,
                            Notes = remoteDataItem.Notes
                        });
                    }
                    else if (localUpdatedAt == null || remoteUpdatedAt > localUpdatedAt)
                    {
                        localData.Name = remoteDataItem.Name;
                        localData.Revenues = parsedRevenues;
                        localData.Payments = parsedPayments;
                        localData.Price = remoteDataItem.Price;
                        localData.OutPrice = remoteDataItem.OutPrice;
                        localData.Payment = remoteDataItem.Payment;
                        localData.Date = DateTime.SpecifyKind(remoteDataItem.Date, DateTimeKind.Utc);
                        localData.IsDeleted = remoteDataItem.IsDeleted;
                        localData.UpdatedAt = remoteUpdatedAt;
                        localData.IsSynced = true;
                        localData.Notes = remoteDataItem.Notes;
                    }

                    remoteDataItem.IsSynced = true;
                }

                await dbContext.SaveChangesAsync();
                await _supabaseClient.From<SupabaseData>().Upsert(remoteData);
            }

            _lastDataSync = fetchTime;
        }
        #endregion

        private async Task DeletedLogs(ElDarDbContext dbContext)
        {
            var deletedItems = await dbContext.DeletedLogs.ToListAsync();

            foreach (var item in deletedItems)
            {
                try
                {
                    switch (item.TableName)
                    {
                        case "Guests":
                            await _supabaseClient.From<SupabaseGuest>().Where(x => x.Id == item.Id).Delete();
                            break;
                        case "Employees":
                            await _supabaseClient.From<SupabaseEmployee>().Where(x => x.Id == item.Id).Delete();
                            break;
                        case "Data":
                            await _supabaseClient.From<SupabaseData>().Where(x => x.Id == item.Id).Delete();
                            break;
                        case "User":
                            await _supabaseClient.From<SupabaseUser>().Where(x => x.Id == item.Id).Delete();
                            break;
                    }
                    await _supabaseClient.From<SupabaseDeletedLogs>().Upsert(new SupabaseDeletedLogs
                    {
                        Id = item.Id,
                        TableName = item.TableName,
                        DeletedAt = DateTime.UtcNow
                    });

                    dbContext.DeletedLogs.Remove(item);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Sync Delete Error for {item.TableName} - {item.Id}]: {ex.ToString()}");
                }
            }

            await dbContext.SaveChangesAsync();

        var remoteDeletes = await _supabaseClient.From<SupabaseDeletedLogs>()
        .Where(d => d.DeletedAt > _lastDeleteSync).Get();

            foreach (var del in remoteDeletes.Models)
            {
                switch (del.TableName)
                {
                    case "Guests":
                        var g = await dbContext.Guests.FindAsync(del.Id);
                        if (g != null) dbContext.Guests.Remove(g);
                        break;
                    case "Employees":
                        var e = await dbContext.Employees.FindAsync(del.Id);
                        if (e != null) dbContext.Employees.Remove(e);
                        break;
                    case "Data":
                        var d = await dbContext.Data.FindAsync(del.Id);
                        if (d != null) dbContext.Data.Remove(d);
                        break;
                    case "User":
                        var u = await dbContext.User.FindAsync(del.Id);
                        if (u != null) dbContext.User.Remove(u);
                        break;
                }
            }
            await dbContext.SaveChangesAsync();
            _lastDeleteSync = DateTime.UtcNow;
        }

        #region
        private async Task SyncUserUpsertsAsync(ElDarDbContext dbContext)
        {
            var unsyncedUsers = await dbContext.User.Where(u => !u.IsSynced).ToListAsync();
            HashSet<Guid> justPushedUserIds = new HashSet<Guid>();

            if (unsyncedUsers.Any())
            {
                var SupabaseUsers = unsyncedUsers.Select(u => new SupabaseUser
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    PasswordHash = u.PasswordHash,
                    ResetPasswordToken = u.ResetPasswordToken,
                    ResetTokenExpires = u.ResetTokenExpires,
                    CreatedAt = DateTime.SpecifyKind(u.CreatedAt, DateTimeKind.Utc),
                    UpdatedAt = u.UpdatedAt.HasValue
                        ? DateTime.SpecifyKind(u.UpdatedAt.Value, DateTimeKind.Utc)
                        : DateTime.SpecifyKind(u.CreatedAt, DateTimeKind.Utc),
                    IsSynced = false
                }).ToList();

                await _supabaseClient.From<SupabaseUser>().Upsert(SupabaseUsers);

                justPushedUserIds = unsyncedUsers.Select(u => u.Id).ToHashSet();
                foreach (var user in unsyncedUsers)
                {
                    user.IsSynced = true;
                }
                await dbContext.SaveChangesAsync();

            }
            var fetchTime = DateTime.UtcNow;
            var supabaseChanges = await _supabaseClient.From<SupabaseUser>()
                .Where(g => g.IsSynced == false || g.UpdatedAt > _lastUserSync)
                .Get();

            var remoteUsers = supabaseChanges.Models
                .Where(rd => !justPushedUserIds.Contains(rd.Id))
                .ToList();

            if (remoteUsers.Any())
            {
                foreach (var remoteUserItem in remoteUsers)
                {
                    var localUser = await dbContext.User.FirstOrDefaultAsync(g => g.Id == remoteUserItem.Id);

                    var rawRemoteDataUpdated = remoteUserItem.UpdatedAt ?? DateTime.UtcNow;
                    var remoteUpdatedAt = DateTime.SpecifyKind(rawRemoteDataUpdated, DateTimeKind.Utc);

                    DateTime? localUpdatedAt = localUser?.UpdatedAt.HasValue == true
                        ? DateTime.SpecifyKind(localUser.UpdatedAt.Value, DateTimeKind.Utc)
                        : null;

                    if (localUser == null)
                    {
                        dbContext.User.Add(new User
                        {
                            Id = remoteUserItem.Id,
                            UserName = remoteUserItem.UserName,
                            PasswordHash = remoteUserItem.PasswordHash,
                            Email = remoteUserItem.Email,
                            ResetPasswordToken = remoteUserItem.ResetPasswordToken,
                            ResetTokenExpires = remoteUserItem.ResetTokenExpires,
                            CreatedAt = DateTime.SpecifyKind(remoteUserItem.CreatedAt, DateTimeKind.Utc),
                            UpdatedAt = remoteUpdatedAt,
                            IsSynced = true,
                        });
                    }
                    else if (localUpdatedAt == null || remoteUpdatedAt > localUpdatedAt)
                    {
                        localUser.PasswordHash = remoteUserItem.PasswordHash;
                        localUser.ResetPasswordToken = remoteUserItem.ResetPasswordToken;
                        localUser.ResetTokenExpires = remoteUserItem.ResetTokenExpires;
                        localUser.UpdatedAt = remoteUpdatedAt;
                        localUser.IsSynced = true;
                    }
                    remoteUserItem.IsSynced = true;
                }

            await dbContext.SaveChangesAsync();
            await _supabaseClient.From<SupabaseUser>().Upsert(remoteUsers);

            }

            _lastUserSync = fetchTime;


        }
    }
}
#endregion