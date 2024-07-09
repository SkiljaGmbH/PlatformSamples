# Sample Activities


## Licensed Activity Sample
A licensed activity requires a license from the activity vendor. The vendor signs the license file with his private key, and puts the public key into the activity itself.
The private key is inside the sampleActivityPrivate_pwd_password.pfx with the sources of the SampleActivities, the password is password.
An appropriate license needs to be created for this activity, otherwise it won't run.
Use the license tool to create such a license, e.g.:
	- Dev.LicenseCreator.exe gena -l 2b93e963-87ab-484b-8eb3-9fc311bb2166 --pfx .\DemoActivities\SampleActivities\sampleActivityPrivate_pwd_password.pfx --password password --vendor "Acme Consulting" --exp 2019-10-30 --payload .\DemoActivities\SampleActivities\custom_license.json -o output.lic

A note on activity license generation:
- the activity type guid must match. If in doubt, confirm that.
- only a cmd line tool exists for now.

## Split Activity Sample
Simple activity to demonstrate the way work items can be split into multiple child work items and processed separately

## Merge Activity Sample
Simple activity to demonstrate the way multiple child work items can be merged back to parent work item.
Activity demonstrates child work items merging but this does not represent a limitation, any work item can be merged with other work item.
