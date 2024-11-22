(@() + (gci -Recurse "obj") + (gci -Recurse "bin")) | % { rm -Recurse $_ }
