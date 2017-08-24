--Version:1.46.0.1
--Description: fixes the non-standard naming of PermissionWindow locks
if exists (select 1 from sys.columns where name = 'LockingProcess')
begin

exec sp_rename 'PermissionWindow.LockingProcess', 'LockHeldBy','Column'

 exec sp_rename 'PermissionWindow.IsLocked', 'LockedBecauseRunning','Column'

 alter table PermissionWindow drop column LockingUser
 end