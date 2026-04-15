(@() + (Get-ChildItem -Recurse "obj") + (Get-ChildItem -Recurse "bin")) | % { Remove-Item -Recurse -Force $_ }
