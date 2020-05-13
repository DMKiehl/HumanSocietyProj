using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace HumaneSociety
{
    public static class Query
    {        
        static HumaneSocietyDataContext db;

        static Query()
        {
            db = new HumaneSocietyDataContext();
        }

        internal static List<USState> GetStates()
        {
            List<USState> allStates = db.USStates.ToList();       

            return allStates;
        }
            
        internal static Client GetClient(string userName, string password)
        {
            Client client = db.Clients.Where(c => c.UserName == userName && c.Password == password).Single();

            return client;
        }

        internal static List<Client> GetClients()
        {
            List<Client> allClients = db.Clients.ToList();

            return allClients;
        }

        internal static void AddNewClient(string firstName, string lastName, string username, string password, string email, string streetAddress, int zipCode, int stateId)
        {
            Client newClient = new Client();

            newClient.FirstName = firstName;
            newClient.LastName = lastName;
            newClient.UserName = username;
            newClient.Password = password;
            newClient.Email = email;

            Address addressFromDb = db.Addresses.Where(a => a.AddressLine1 == streetAddress && a.Zipcode == zipCode && a.USStateId == stateId).FirstOrDefault();

            // if the address isn't found in the Db, create and insert it
            if (addressFromDb == null)
            {
                Address newAddress = new Address();
                newAddress.AddressLine1 = streetAddress;
                newAddress.City = null;
                newAddress.USStateId = stateId;
                newAddress.Zipcode = zipCode;                

                db.Addresses.InsertOnSubmit(newAddress);
                db.SubmitChanges();

                addressFromDb = newAddress;
            }

            // attach AddressId to clientFromDb.AddressId
            newClient.AddressId = addressFromDb.AddressId;

            db.Clients.InsertOnSubmit(newClient);

            db.SubmitChanges();
        }

        internal static void UpdateClient(Client clientWithUpdates)
        {
            // find corresponding Client from Db
            Client clientFromDb = null;

            try
            {
                clientFromDb = db.Clients.Where(c => c.ClientId == clientWithUpdates.ClientId).Single();
            }
            catch(InvalidOperationException e)
            {
                Console.WriteLine("No clients have a ClientId that matches the Client passed in.");
                Console.WriteLine("No update have been made.");
                return;
            }
            
            // update clientFromDb information with the values on clientWithUpdates (aside from address)
            clientFromDb.FirstName = clientWithUpdates.FirstName;
            clientFromDb.LastName = clientWithUpdates.LastName;
            clientFromDb.UserName = clientWithUpdates.UserName;
            clientFromDb.Password = clientWithUpdates.Password;
            clientFromDb.Email = clientWithUpdates.Email;

            // get address object from clientWithUpdates
            Address clientAddress = clientWithUpdates.Address;

            // look for existing Address in Db (null will be returned if the address isn't already in the Db
            Address updatedAddress = db.Addresses.Where(a => a.AddressLine1 == clientAddress.AddressLine1 && a.USStateId == clientAddress.USStateId && a.Zipcode == clientAddress.Zipcode).FirstOrDefault();

            // if the address isn't found in the Db, create and insert it
            if(updatedAddress == null)
            {
                Address newAddress = new Address();
                newAddress.AddressLine1 = clientAddress.AddressLine1;
                newAddress.City = null;
                newAddress.USStateId = clientAddress.USStateId;
                newAddress.Zipcode = clientAddress.Zipcode;                

                db.Addresses.InsertOnSubmit(newAddress);
                db.SubmitChanges();

                updatedAddress = newAddress;
            }

            // attach AddressId to clientFromDb.AddressId
            clientFromDb.AddressId = updatedAddress.AddressId;
            
            // submit changes
            db.SubmitChanges();
        }
        
        internal static void AddUsernameAndPassword(Employee employee)
        {
            Employee employeeFromDb = db.Employees.Where(e => e.EmployeeId == employee.EmployeeId).FirstOrDefault();

            employeeFromDb.UserName = employee.UserName;
            employeeFromDb.Password = employee.Password;

            db.SubmitChanges();
        }

        internal static Employee RetrieveEmployeeUser(string email, int employeeNumber)
        {
            Employee employeeFromDb = db.Employees.Where(e => e.Email == email && e.EmployeeNumber == employeeNumber).FirstOrDefault();

            if (employeeFromDb == null)
            {
                throw new NullReferenceException();
            }
            else
            {
                return employeeFromDb;
            }
        }

        internal static Employee EmployeeLogin(string userName, string password)
        {
            Employee employeeFromDb = db.Employees.Where(e => e.UserName == userName && e.Password == password).FirstOrDefault();

            return employeeFromDb;
        }

        internal static bool CheckEmployeeUserNameExist(string userName)
        {
            Employee employeeWithUserName = db.Employees.Where(e => e.UserName == userName).FirstOrDefault();

            return employeeWithUserName != null;
        }


        //// TODO Items: ////
        
        // TODO: Allow any of the CRUD operations to occur here
        internal static void RunEmployeeQueries(Employee employee, string crudOperation)
        {
            Employee EmployeeFromDB = null;
            switch (crudOperation)
            {
                case "read":
                    EmployeeFromDB= db.Employees.Where(e => e.EmployeeNumber == employee.EmployeeNumber).SingleOrDefault();
                    UserInterface.DisplayEmployeeInfo(EmployeeFromDB);
                    break;
                case "delete":
                    EmployeeFromDB = db.Employees.Where(e => e.EmployeeNumber == employee.EmployeeNumber).SingleOrDefault();
                    db.Employees.DeleteOnSubmit(EmployeeFromDB);
                    db.SubmitChanges();
                    break;
                case "create":
                    db.Employees.InsertOnSubmit(employee);
                    db.SubmitChanges();
                    break;
                case "update":
                    EmployeeFromDB = db.Employees.Where(e => e.EmployeeNumber == employee.EmployeeNumber).SingleOrDefault();
                    EmployeeFromDB.FirstName = employee.FirstName;
                    EmployeeFromDB.LastName = employee.LastName;
                    EmployeeFromDB.Email = employee.Email;
                    db.SubmitChanges();  
                    break;
            }

        }

        // TODO: Animal CRUD Operations
        internal static void AddAnimal(Animal animal)
        {
            db.Animals.InsertOnSubmit(animal);
            db.SubmitChanges();
        }

        internal static Animal GetAnimalByID(int id)
        {
            Animal animal = db.Animals.Where(g => g.AnimalId == id).SingleOrDefault();
            return animal;
        }

        internal static void UpdateAnimal(int animalId, Dictionary<int, string> updates)
        {
            Animal animals = db.Animals.Where(a => a.AnimalId == animalId).SingleOrDefault();

            foreach (KeyValuePair<int, string> item in updates)
            {
                switch (item.Key)
                {
                    case 1:
                        int categoryID = GetCategoryId(item.Value);
                        animals.CategoryId = categoryID;
                        break;
                    case 2:
                        animals.Name = item.Value;
                        break;
                    case 3:
                        int age = int.Parse(item.Value);
                        animals.Age = age;
                        break;
                    case 4:
                        animals.Demeanor = item.Value;
                        break;
                    case 5:
                        if (item.Value == "yes")
                        {
                            animals.KidFriendly = true;
                        }
                        else
                        {
                            animals.KidFriendly = false;
                        }
                        break;
                    case 6:
                        if (item.Value == "yes")
                        {
                            animals.PetFriendly = true;
                        }
                        else
                        {
                            animals.PetFriendly = false;
                        }
                        break;
                    case 7:
                        int weight = int.Parse(item.Value);
                        animals.Weight = weight;
                        break;
                }
            }
        }

        internal static void RemoveAnimal(Animal animal)
        {
            db.Animals.DeleteOnSubmit(animal);
            db.SubmitChanges();
        }

        // TODO: Animal Multi-Trait Search
        internal static IQueryable<Animal> SearchForAnimalsByMultipleTraits(Dictionary<int, string> updates) // parameter(s)?
        {
            IQueryable<Animal> animals = db.Animals;
            foreach (KeyValuePair<int, string> item in updates)
            {
                switch (item.Key)
                {
                    case 1:
                        int categoryID = GetCategoryId(item.Value);
                        animals = animals.Where(a => a.CategoryId == categoryID);
                        break;
                    case 2:
                        animals = animals.Where(a => a.Name == item.Value);
                        break;
                    case 3:
                        int age = int.Parse(item.Value);
                        animals = animals.Where(a => a.Age == age);
                        break;
                    case 4:
                        animals = animals.Where(a => a.Demeanor == item.Value);
                        break;
                    case 5:
                        if (item.Value == "yes")
                        {
                            animals = animals.Where(a => a.KidFriendly == true);
                        }
                        else
                        {
                            animals = animals.Where(a => a.KidFriendly == false);
                        }
                        break;
                    case 6:
                        if (item.Value == "yes")
                        {
                            animals = animals.Where(a => a.PetFriendly == true);
                        }
                        else
                        {
                            animals = animals.Where(a => a.PetFriendly == false);
                        }
                        break;
                    case 7:
                        int weight = int.Parse(item.Value);
                        animals = animals.Where(a => a.Weight == weight);
                        break;
                    case 8:
                        int ID = int.Parse(item.Value);
                        animals = animals.Where(a => a.AnimalId == ID);
                        break;
                }
            }           
            return animals;
        }

        // TODO: Misc Animal Things
        internal static int GetCategoryId(string categoryName)
        {
            int getCategoryId = db.Categories.Where(c => c.Name == categoryName).Select(c => c.CategoryId).SingleOrDefault();
            return getCategoryId;
        }
        
        internal static Room GetRoom(int animalId)
        {
            Room roomNumber = db.Rooms.Where(r => r.AnimalId == animalId).SingleOrDefault();
            return roomNumber;
        }
        
        internal static int GetDietPlanId(string dietPlanName)
        {
            int getDietPlanId = db.DietPlans.Where(g => g.Name == dietPlanName).Select(g => g.DietPlanId).SingleOrDefault();
            return getDietPlanId;
        }

        // TODO: Adoption CRUD Operations
        internal static void Adopt(Animal animal, Client client)
        {
            throw new NotImplementedException();
        }

        internal static IQueryable<Adoption> GetPendingAdoptions()
        {
            throw new NotImplementedException();
        }

        internal static void UpdateAdoption(bool isAdopted, Adoption adoption)
        {
            throw new NotImplementedException();
        }

        internal static void RemoveAdoption(int animalId, int clientId)
        {
            Animal animal = db.Animals.Where(a => a.AnimalId == animalId).Single();
            animal.AdoptionStatus = "not adopted";
            Adoption adoption = db.Adoptions.Where(a => a.AnimalId == animalId && a.ClientId == clientId).SingleOrDefault();
            db.Adoptions.DeleteOnSubmit(adoption);
            db.SubmitChanges();
        }

        // TODO: Shots Stuff
        internal static IQueryable<AnimalShot> GetShots(Animal animal)
        {
            throw new NotImplementedException();
        }

        internal static void UpdateShot(string shotName, Animal animal)
        {
            throw new NotImplementedException();
        }
    }
}