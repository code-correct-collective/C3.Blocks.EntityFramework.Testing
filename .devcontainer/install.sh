sudo apt-get update
sudo apt-get upgrade -y


echo "************************"
echo "* Install dependencies *"
echo "************************"
dotnet tool restore # Restore husky in the root of the monorepo
dotnet husky install # Install the git hooks
dotnet tool restore
dotnet restore
