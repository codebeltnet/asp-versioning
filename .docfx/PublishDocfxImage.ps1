$version = minver -i -t v -v w
docker tag asp-versioning-docfx:$version jcr.codebelt.net/geekle/asp-versioning-docfx:$version
docker push jcr.codebelt.net/geekle/asp-versioning-docfx:$version
