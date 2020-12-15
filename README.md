# MatBox
All business logic of the app is contained in three controllers:
1. AdminController - different actions on users and their roles (only admins have access).
2. AccountController - various actions for users of simple categories (reader, writer, no role).
3. MaterialsController - methods for working with WebAPI

# How to run
1. Create MatBox image: "docker image build -t matbox ."
2. Run server with database: "docker-compose up"
3. Create user in /Account/Register
4. Get rights for your user here /Account/Edit?name=userName (for the changes to take effect, you need to log out and log in again)
5. Enjoy!

# WebAPI methods
Available on /api/materials/[methodName]
1. getAllMaterials - will return all materials that are stored in the application.
2. getInfoAboutMaterial - will return information about all versions of the material (you must pass materialName in the request body)
3. getInfoWithFilters - will return information about all versions of materials of a certain category and size (you must pass them in the request body)
4. getActualMaterial - will return the latest version of the material for download (you must pass the materialName in the request body)
5. getSpecificMaterial - will return a specific version of the material for download (you must pass the name and version in the request body)
6. addNewMaterial - adds new material to the app (in the request body, you must pass the file and it's category. Possible categories of material: Презентация, Приложение, Другое)
7. addNewVersionOfMaterial - adds new version of material to the app (in the request body, you must pass the file)
8. changeCategoryOfMaterial - changes the category of the material in all versions (in the request body, you must pass the materialName and newCategory)