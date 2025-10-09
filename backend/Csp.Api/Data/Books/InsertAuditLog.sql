INSERT INTO audit_log (ActorUserId, Action, TargetUserId, Details) 
VALUES (@ActorUserId, @Action, @TargetUserId, @Details)