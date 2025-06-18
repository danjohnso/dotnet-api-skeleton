# Skeleton.Identity
Currently configured for PostgreSQL

The Identity project will need a dependency on the database provider project of choice

This implementation adds some common User properties, auditing events, as well as password history.  
I don't like using roles with claims, since the roles really are claims most of the time, so those are not included in the IdentityContext.