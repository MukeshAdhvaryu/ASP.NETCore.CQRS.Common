# ASP.NETCore.CQRS.Common
Provides template for creating CQRS project faster and efficiently.

## dotnet new install ASP.NETCore.Dynamic.CQRS 

then anywhere you want to create the template use:

## dotnet new install cqrs
OR
## dotnet new install min

Using a Minimal API template for creating a CQRS application is a faster and more efficient way.
Creating a minimal API by choosing from .NET templates is a standard way to get started. 

### I have not included comments and excluded some of the code in this file to keep it short. Actual code has all. Please be rest assured that, I am not a Spaghetti maker!

## Index
[WHY?](#WHY)

[WHAT?](#WHAT)

[HOW?](#HOW)

[WHAT NEXT?](#What_Next)

[General Design](#General_Design)

[Design Of Model](#Design_Of_Model)

[TASK1: Common test project for all three frameworks i.e. xUnit, NUnit or MSTest.](#TASK1)

[TASK2: Perform search for multiple models using multiple search parameters added.](#TASK2)

[TASK3: Support for ClassData and MemberData test attributes added.](#TASK3)

[TASK4: Exception Middleware.](#TASK4) 

[TASK5: Choose database at model level.](#TASK5) 

[TASK6: Defining API End-points.](#TASK6) 

[TASK7: Converted DBContext from generic to non-generic class.](#TASK7)

[TASK8: Support for Query-Only-Controllers and Keyless models is added.](#TASK8)

[TASK9: Abstract Models for common primary key type: int, long, Guid, enum are added.](#TASK9)

[TASK10: ADD: Support for List based (non DbContext) Singleton CQRS.](#TASK10)

[What Next](#What_Next)

## WHY?
We already know that a minimal API can have standard HTTP calls such as HttpGet, HttpPost, etc.
So, we know the possible methods of the service class to expose our API gateway endpoints. 

The problem is: definition of model (entity) which can differ from project to project as different business domains have different entities.  
However, it pained me to start a new project from 'WeatherForecast' template and then write almost everything from the scratch.

We do need something better than that. A template powerful enough to get adapted in most common cases with bare minimum modifications.

[GoTo Index](#Index)

## WHAT?
On one fine day, it dawned upon me that I need to explore a possibility of creating something which addresses the moving part of the minimal API design: Model.
The goal I set was: to make a model centric project where everything which I will need: 
a DbContext, a repository, an exception middleware can be defined and controlled at the model level only.

The hard part was to define a generic DbContext to work for any given model.
A generic service repository should not be a problem though.

### I wanted that a user need to do the minimum to get every thing bound together and work without any issue.

### I also wanted that the user should have a choice to define a contract of operations; 
for example:
To have or not to have a read-only (query) contract generated dynamically.
To have or not to have a write-only (command) contract generated dynamically. 

### TDD is also an integral part of any project. Creating a common template that handles the three most prominent testing frameworks (xUNIT, NUNIT and MSTest) should be an apt thing to do.

The goal was also to include a common test project to handle all three without the user need to change much except any custom test they want to write.
It would be an apt thing to do to define custom attributes to map important attributes from all three testing frameworks.

### Support for keyless (query) models was also to be provided.

### Keyed models should be flexible enough to use various common keys such as int, long, Guid, enum etc.

### To handle under-fetching and over-fetching problems,
Option was to be provided to use DTOs as input argument in POST/PUT and as an output argument in GET calls.

[GoTo Index](#Index)

## HOW

Minimal API comes with some limitations mainly the following:
1. We are not supposed to use controllers.
2. No Model binding using IModelBinder.
3. Some may consider even using service repository a sin, I am not one of them.
   Using DbContext directly without an abstraction layer can never be a good design.

Without model binding, We are in a fix. Especially, when Swagger does not allow Body support in GET.
I am using SearchParamter - a body via FindAll() method to fetch records matching search criteria.
Thankfully, we have some help there:

For any entity we need model binding we need to have the following method be placed inside the entity:

    public static TryParse(string query, out Entity result)
    {
        //read from the query and generate concrete entity.
    }
### Problem is: doing it for each entity is an extra work, quite unnecessary since all we need is: to parse query string using JsonSerializer into a concrete entity.

Now how do we avoid this necessary evil of re-coding?

### Answer is:

    public readonly struct Parser<T>: IParser
    {
        [Flags]
        enum TypeInfo : byte
        {
            None,
            IsEnumerable = 0x1,
            IsParsable = 0x2,
        }

        public readonly T? Result;
        static readonly Type Type;
        static readonly TypeInfo Info; 
        
        static Parser()
        {
            Type = typeof(T);
            Info = 0;

            if (Type.IsAssignableTo(typeof(IEnumerable)))
                Info |= TypeInfo.IsEnumerable;
            if(Type.IsAssignableTo(typeof(ISelfParser<T>)) && (Type.IsValueType || Type.GetConstructor(Type.EmptyTypes) != null))
                Info |= TypeInfo.IsParsable;
        }
        public Parser(T? result)
        {
            Result = result;
        }
        
        public static bool TryParse(string json, out Parser<T> parser)
        {
            json = json.Trim();
            if ((Info & TypeInfo.IsEnumerable) == TypeInfo.IsEnumerable)
            {
                if (!json.StartsWith("["))
                    json = "[" + json;
                if (!json.EndsWith("]"))
                    json += "]";
            }
            T? result;
            if ((Info & TypeInfo.IsParsable) == TypeInfo.IsParsable)
            {
                var parsable = Activator.CreateInstance(Type);
                if(parsable != null)
                {
                    var instance = (ISelfParser<T>)parsable;
                    result = instance.Parse(json);
                    parser = new Parser<T>(result);
                    return true;
                }
            } 
            result = Globals.Parse<T>(json, Type);
            parser = new Parser<T>(result);
            return true;
        } 
    }

### Please note that we have not snatched away an opportunity for the given entity to parse json by itself by providing it through ISelfParser<T> interface. If, it chooses to implement that.

Then the static class Globals:

    public static partial class Globals
    {   
        public static readonly JsonSerializerOptions JsonSerializerOptions;
        static readonly object GlobalLock = new object();
        internal static readonly string Url;
        static volatile bool isProductionEnvironment;

        static Globals()
        {
            lock (GlobalLock)
            {
                JsonSerializerOptions = new JsonSerializerOptions().AddDefaultOptions();
                Url = @"/{0} /{1}";
            }
        }
        
        public static bool IsProductionEnvironment 
        { 
           get => isProductionEnvironment;
           internal set => isProductionEnvironment = value; 
       }
                
        public static JsonSerializerOptions AddDefaultOptions(this JsonSerializerOptions JsonSerializerOptions)
        {
            JsonSerializerOptions.IncludeFields = true;
            JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            JsonSerializerOptions.IgnoreReadOnlyFields = true;
            JsonSerializerOptions.IgnoreReadOnlyProperties = true;
            JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            return JsonSerializerOptions;
        }
       
        public static T? Parse<T>(string json, Type type)
        {
            try
            {
                var result = JsonSerializer.Deserialize(json, type, JsonSerializerOptions);
                if (result == null)
                    return default(T);
                return (T)result;
            }
            catch
            {
                throw;
            }
        }
        
        // other useful methods...        
    }
### Now consider the following methods defined in Command and Query Service classes which are our surrogates in lieu of controllers.

### Command Service:

    partial class CommandService<TOutDTO, TModel, TID, TInDTO, TContext> : IExContract
    {
        protected override void Configure(IEndpointRouteBuilder app)
        {
        #if MODEL_APPENDABLE
            app.MapPost(GetUrl("Add"), [Tags("Command")] async(Parser<TInDTO?> model) =>
                await Command.Add(model.Result));
        #endif
        
        #if (MODEL_APPENDABLE) && MODEL_APPENDBULK
            app.MapPost(GetUrl("AddBulk"), [Tags("Command")] async (Parser<IEnumerable<TInDTO?>?> models) =>
                await Command.AddRange(models.Result));
        #endif     
        }
    }

    partial class QueryService<TOutDTO, TModel, TContext> : IExContract
    {
        protected override void Configure(IEndpointRouteBuilder app)
        {
        #if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
            app.MapGet(GetUrl("GetAll/{count}"), [Tags("Query")] async (int? count) =>
                await Query.GetAll(count ?? 0));

            app.MapGet(GetUrl("GetPortion/{startIndex}, {count}"), [Tags("Query")] async (int startIndex, int? count) =>
                await Query.GetAll(startIndex, count ?? 0));

        #if MODEL_SEARCHABLE
            app.MapGet(GetUrl("Find"), [Tags("Query")] async (Parser<SearchParameter[]> parameters, AndOr? join) =>
                await Query.Find(join ?? AndOr.OR, parameters.Result));

            app.MapGet(GetUrl("FindAll"), [Tags("Query")] async (Parser<SearchParameter[]> parameters, AndOr? join) =>
                await Query.FindAll(join ?? AndOr.OR, parameters.Result));
        #endif
        #endif
        }
    }

### Do you see the trick? We have wrapped our actual parameters in Parser<T> so now we have made it sure that: 
1. We do not get 'No TryParse method found!' error, because Parser<T> has it well defined.
2. We parse the json and store the result in parser class.
3. We pass the result to actual Command and Query object.
4. Thus, we get the job done without writing the same annoying TryParse method in every entity that we define.

### CAUTION: Do not use \[FromQuery] or \[FromBody] in end-points. If you use those, TryParse will not get called and an ugly error will be generated.

To provide supports for the above mentioned, the following CCC (Conditional Compilation Constants) were came to my mind:

To control a contract of operations on a project-to-project basis:
1. MODEL_APPENDABLE - this is to have HttpPost capability.
2. MODEL_DELETABLE - this is to have HttpDelete capability.
3. MODEL_UPDATABLE - this is to have HttpPut capability.
4. MODEL_NONREADABLE  Or MODEL_NONQUERYABLE - this is to skip HttpGet methods from getting defined in query service. 

For testing:
1. TDD - this is to stay in TDD environment (before writing any ASP WEB-API code).
2. MODEL_ADDTEST - this is to enable support for service and standard test templates
3. TEST_USERMODELS - this is to test actual models defined by the user as opposed to test model provided.
4. MODEL_USEXUNIT - this is to choose xUnit as testing framework.
5. MODEL_USENUNIT - this to choose NUnit as testing framework

In absence of any of the last two CCCs, MSTest should be used as default test framework.

To handle under-fetching and over-fetching:

1. MODEL_USEDTO - this is to allow the usage of DTOs.

    
The following Generic Type definitions are used throughout the project:

     TID where TID : struct. (To represent a primary key type of the model - no string key support sorry!).
     TOutDTO  where TOutDTO : IModel, new() (Root interface of all models).
     TInDTO  where TInDTO : IModel, new() (Root interface of all models).
     
     TModel where TModel : class, ISelfModel<TID, TModel>, new()
     #if (!MODEL_USEDTO)
     , TOutDTO,
     #endif
     
     For keyless support:
     TModel where TModel : class, ISelfModel<TModel>, new()
 

Concrete (non-abstract) keyed model defined by you must derive from an abstract Model\<TID, TModel\> class provided.
Concrete (non-abstract) keyless model defined by you must derive from an abstract Model\<TModel\> class provided.

you can choose to provide your own Command and Query implementation by:
Inheriting from and then overriding methods of Query\<TOutDTO, TModel\> and Command\<TOutDTO, TModel\> classes respectively.

If you want your model to provide seed data when DBContext is empty then.. 
1. You must override GetInitialData() method of the Model\<TModel\> class to provide a list of created models.
2. Adorn your model with attribute: \[Model(ProvideSeedData = true)].

If you want your model to specify a scope of attached service then.. 
1.  Adorn your model with attribute: \[Model(Scope = ServiceScope.Your Choice)].

By default, DBContext uses InMemory SqlLite by using "InMemory" connection string stored in configuration.

I haven't touched data migration simply because I am not so on-board with code-first approach.
To me, alllowing database changes from the code may turn into bad practice. But that is just me!

[GoTo Index](#Index)

## General_Design 

1. Defined an abstract layer called CQRS.Common
    This layer will have no awareness of any Minimal API endpoints or DbContext. 
    One should be able to use it for something else.  
    In TDD mode, all we need is to make sure that contract operations on a given model works.
    We should be able to create test projects before we even create an actual Web API project.
   
 3. Operation contracts are defined in two categories:
     a. Query contract (read-only)
     b. Command contract (write-only)
  
 4. Command and Query contract interfaces:
 
        public interface ICommandContract<TOutDTO, TModel, TID> : IContract            
        where TOutDTO : IModel, new()
        where TModel : class, ISelfModel<TID, TModel>,
            #if (!MODEL_USEDTO)
                TOutDTO,
            #endif
        new()
        where TID : struct
        {
        #if (MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE)
            ICommand<TOutDTO, TModel, TID> Command { get; }
        #endif
        }
        
        public interface IQueryContract<TOutDTO, TModel> : IContract           
        where TOutDTO : IModel, new()
        where TModel : ISelfModel<TModel>, new()
            #if (!MODEL_USEDTO)
                , TOutDTO
            #endif
        
        {
         #if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
              IQuery<TOutDTO, TModel> Query { get; }
        #endif
        }

        public interface IQueryContract<TOutDTO, TModel, TID> : 
        IQueryContract<TOutDTO, TModel>
        #region TYPE CONSTRINTS
        where TOutDTO : IModel, new()
        where TModel : class, ISelfModel<TID, TModel>,
        #if (!MODEL_USEDTO)
            TOutDTO,
        #endif 
        new()
        where TID : struct
        {
        #if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE)
              new IQuery<TOutDTO, TModel, TID> Query { get; }
        #endif 
        }
    
 5. Command and Query service interfaces:
    
        #if MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE
            public interface ICommand<TOutDTO, TModel, TID> : IModelCount
                #if MODEL_APPENDABLE
                    , IAppendable<TOutDTO, TModel, TID>
                #endif
                #if MODEL_UPDATABLE
                    , IUpdatable<TOutDTO, TModel, TID>
                #endif
                #if MODEL_DELETABLE
                    , IDeleteable<TOutDTO, TModel, TID>
                #endif

            where TOutDTO : IModel, new()
            where TModel : class, ISelfModel<TID, TModel>, new()
                #if (!MODEL_USEDTO)
                    , TOutDTO
                #endif
            where TID : struct
            {

            }
        #endif

        #if !MODEL_NONREADABLE || !MODEL_NONQUERYABLE
            public partial interface IQuery<TOutDTO, TModel> : IModelCount, IFind<TOutDTO, TModel>, IFirstModel<TModel>
            where TModel : ISelfModel<TModel>
            where TOutDTO : IModel, new()
            { 
            }

            public interface IQuery<TOutDTO, TModel, TID> : IQuery<TOutDTO, TModel>, IFindByID<TOutDTO, TModel, TID>
            where TOutDTO : IModel, new()
            where TModel : class, ISelfModel<TID, TModel>, new()
                #if (!MODEL_USEDTO)
                    , TOutDTO
                #endif
            where TID : struct
            {
            }
        #endif

 7. Single repsonsibility interfaces:
      1. IAppendable\<TOutDTO, TModel, TID\>
      2. IUpdatable\<TOutDTO, TModel, TID\>
      3. IDeleteable\<TOutDTO, TModel, TID\>
      4. IFindByID\<TOutDTO, TModel, TID\>
      5. IFetch\<TOutDTO, TModel\>
      6. ISearch\<TOutDTO, TModel\>
      7. IParser
      8. ISelfParser\<T\>
           
        #if !MODEL_NONREADABLE || !MODEL_NONQUERYABLE
            public interface IFetch<TOutDTO, TModel>
            where TOutDTO : IModel, new()
            where TModel : IModel
            {        
                Task<IEnumerable<TOutDTO>?> GetAll(int count = 0);
                Task<IEnumerable<TOutDTO>?> GetAll(int startIndex, int count);
            }
            #endif
    
            #if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE) && MODEL_SEARCHABLE
                public interface ISearch<TOutDTO, TModel>
                where TOutDTO : IModel, new()
                where TModel : IModel
                {
                    Task<TOutDTO?> Find<T>(AndOr conditionJoin, params T?[]? parameters)
                    where T : ISearchParameter;
            
                    Task<IEnumerable<TOutDTO>?> FindAll<T>(AndOr conditionJoin, params T?[]? parameters)
                    where T : ISearchParameter;
                }
            #endif

        #if MODEL_APPENDABLE
              public interface IAppendable<TOutDTO, TModel, TID>
              #region TYPE CONSTRINTS
              where TOutDTO : IModel, new()
              where TModel : ISelfModel<TID, TModel>,
              #if (!MODEL_USEDTO)
              TOutDTO,
              #endif
              //+:cnd:noEmit
              new()
              where TID : struct
              #endregion
              {        
                  Task<TOutDTO?> Add(IModel? model);
              #if MODEL_APPENDBULK 
                  Task<Tuple<IEnumerable<TOutDTO?>?, string>> AddRange<T>(IEnumerable<T?>? models)
                  where T: IModel;
              #endif 
              }
        #endif

        #if MODEL_UPDATABLE
            public interface IUpdatable<TOutDTO, TModel, TID> 
            where TOutDTO : IModel, new()
            where TModel : ISelfModel<TID, TModel>,
            #if (!MODEL_USEDTO)
            TOutDTO,
            #endif 
            new()
            where TID : struct 
            {         
                Task<TOutDTO?> Update(TID id, IModel? model); 
            #if MODEL_UPDATEBULK 
                Task<Tuple<IEnumerable<TOutDTO>?, string>> UpdateRange<T>(IEnumerable<TID>? IDs, IEnumerable<T?>? models)
                where T: IModel;
            #endif
            }
        #endif
        #if MODEL_DELETABLE
            public interface IDeleteable<TOutDTO, TModel, TID> 
            where TOutDTO : IModel, new()
            where TModel : ISelfModel<TID, TModel>,
            #if (!MODEL_USEDTO)
            TOutDTO,
            #endif 
            new()
            where TID : struct 
            {
                Task<TOutDTO?> Delete(TID id); 
            #if MODEL_DELETEBULK 
                Task<Tuple<IEnumerable<TOutDTO>?, string>> DeleteRange(IEnumerable<TID>? IDs);
            #endif
            }
        #endif

        #if !MODEL_NONREADABLE || !MODEL_NONQUERYABLE    
            public interface IFindByID<TOutDTO, TModel, TID> 
            where TOutDTO : IModel, new()
            where TModel : ISelfModel<TID, TModel> 
            #if (!MODEL_USEDTO)
              , TOutDTO
            #endif 
            where TID : struct
            {          
                Task<TOutDTO?> Get(TID? id);
            }
        #endif

        public interface IParser
        { }

        public interface ISelfParser<T>: IParser
        {         
            T? Parse(string json);
        }     
       
[GoTo Index](#Index)

## Design: Model
   1. IModel
   2. IModel\<TID\>
   3. ISelfModel\<TModel\>
   4. ISelfModel\<TID, TModel\>
      
    public interface IModel
    { 
    }    
    public interface IModel<TID> : IModel, IMatch
        where TID : struct
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        TID ID { get; }
    }
    public partial interface ISelfModel<TModel> : IModel, IMatch
        where TModel : ISelfModel<TModel>
    {
    }
    
    public partial interface ISelfModel<TID, TModel> : ISelfModel<TModel>, IModel<TID>
        where TModel : ISelfModel<TID, TModel>, IModel<TID>
        where TID : struct
    {
    }

As we already talked about a model centric approach, the following internal interfaces are the key to achieve that.
    1. IExModel 
    2. IExModel\<TID\>

    internal partial interface IExModel : IModel, IExCopyable, IExParamParser, IExModelExceptionSupplier
    #if MODEL_USEDTO
        , IExModelToDTO
    #endif
    {       
        IEnumerable<IModel> GetInitialData();
    }     
    internal interface IExModel<TID> : IModel<TID>, IExModel
        where TID : struct
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        new TID ID { get; set; } 
        
        TID GetNewID(); 
        
        bool TryParseID(object value, out TID newID);
    }
    #endregion

Single repsonsibility interfaces:
   1. IEntity
   2. IExCopyable
   3. IExParamParser
   4. IExModelExceptionSupplier       

    public interface IEntity: IModel
    {
        object? this[string? propertyName] { get; }
    }
    
    internal interface IExCopyable
    {
        Task<bool> CopyFrom(IModel model);
    }

    internal interface IExParamParser
    {
        bool Parse(string? propertyName, object? propertyValue, out object? parsedValue, bool updateValueIfParsed = false, Criteria criteria = 0);
    }
    
    internal interface IExModelExceptionSupplier
    {
       ModelException GetModelException(ExceptionType exceptionType, string? additionalInfo = null, Exception? innerException = null);
    }

Now consider an implementation of all of the above to conjure up the model centric design:

    public abstract partial class Model<TModel> : ISelfModel<TModel>, IExModel, IMatch
        where TModel : Model<TModel>, ISelfModel<TModel>
    {
         readonly string modelName;  
        protected Model()
        {
            var type = GetType();
            modelName = type.Name;
        }
       
        public string ModelName => modelName;
        
        protected abstract Message Parse(IParameter parameter, out object? currentValue, out object? parsedValue, bool updateValueIfParsed = false);
        bool IExParamParser.Parse(IParameter parameter, out object? currentValue, out object? parsedValue, bool updateValueIfParsed, Criteria criteria)
        {
            var name = parameter.Name;
            parsedValue = null;
            object? value;
            
            switch (name)
            {
                default:
                    switch (criteria)
                    {
                       /* 
                        Handles string based criteria. We only need to convert value to string.
                        parsedValue = value.ToString();
                        return true;
                        For other type of criterias, we need to do parsing.

                       */
                    }
                    break;
            }
            return Parse(propertyName, propertyValue, out parsedValue, updateValueIfParsed, criteria);
        }
        
        protected abstract Task<bool> CopyFrom(IModel model);
        Task<bool> IExCopyable.CopyFrom(IModel model) =>
            CopyFrom(model);            
       
        protected abstract IEnumerable<IModel> GetInitialData();
        IEnumerable<IModel> IExModel.GetInitialData() =>
            GetInitialData();

        protected virtual string GetModelExceptionMessage(ExceptionType exceptionType, string? additionalInfo = null)
        {
            bool noAdditionalInfo = string.IsNullOrEmpty(additionalInfo);

            switch (exceptionType)
            {
                // Provides tailor made message for given exception type.

                default:
                    return ("Need to supply your message");
            }
        }
        string IExModelExceptionSupplier.GetModelExceptionMessage(ExceptionType exceptionType, string? additionalInfo, Exception? innerException) =>
            GetAppropriateExceptionMessage(exceptionType, additionalInfo, innerException);
            
        #if MODEL_USEDTO
            protected virtual IModel? ToDTO(Type type)
            {
                var t = GetType();
                if (type == t || t.IsAssignableTo(type))
                    return this;
                return null;
            }
            IModel? IExModelToDTO.ToDTO(Type type) =>
                ToDTO(type);
            #endregion
        #endif
    }

    public abstract partial class Model<TID, TModel> : Model<TModel>,
        IExModel<TID>, IExModel, ISelfModel<TID, TModel>
        where TID : struct
        where TModel : Model<TID, TModel>
    {
        TID id;
        protected Model(bool generateNewID)
        {
            if (generateNewID)
                id = GetNewID();
        }
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public TID ID { get => id; protected set => id = value; }
       
        TID IExModel<TID>.ID { get => id; set => id = value; }
        Task<bool> IExCopyable.CopyFrom(IModel model)
        {
            if (model is IModel<TID>)
            {
                var m = (IModel<TID>)model;
                if (Equals(id, default(TID)))
                    id = m.ID;
                return CopyFrom(model);
            }

            #if MODEL_USEDTO
            if (model is IModel)
            {
                if (Equals(id, default(TID)))
                    id = GetNewID();
                return CopyFrom(model);
            }            
            #endif
            return Task.FromResult(false);
        }
        #endregion
        
        protected abstract TID GetNewID();
        TID IExModel<TID>.GetNewID() =>
            GetNewID();
            
        protected virtual bool TryParseID(object? propertyValue, out TID id)
        {
            id = default(TID);
            return false;
        }
        bool IExModel<TID>.TryParseID(object? propertyValue, out TID id)
        {
            if (propertyValue is TID)
            {
                id = (TID)(object)propertyValue;
                return true;
            }
            if (propertyValue == null)
            {
                id = default(TID);
                return false;
            }
            var value = propertyValue?.ToString();
            id = default(TID);

            if (string.IsNullOrEmpty(value))
                return false;

            switch (IDType)
            {
                 case IDType.Int32:
                    if (int.TryParse(value, out int iResult))
                    {
                        id = (TID)(object)iResult;
                        return true;
                    }
                    break;
                /*
                   Provides parsing for other known types such as long, byte, sbyte, uint, ulong, Guid etc.
                    For any custom ID Type you will need to override TryParseID method for parsing successfully.
                */
                
                default:
                    break;
            }

            //Handles custom ID types.
            return TryParseID(propertyValue, out id);
        }
    }


That's it. 

[GoTo Index](#Index)

## TASK1 
A single test project is created for each TDD and Non TDD environment.
One for testing a service in TDD environment:

        public abstract class ServiceTest<TOutDTO, TModel, TID>  
        where TOutDTO : IModel, new()
        where TModel : Model<TID, TModel>,
        #if (!MODEL_USEDTO)
        TOutDTO,
        #endif
        new()
        where TID : struct
        {
            readonly IContract<TOutDTO, TModel, TID> Contract;
            protected readonly IFixture Fixture;
    
            static readonly IExModelExceptionSupplier DummyModel = new TModel();
            #if MODEL_USEDTO
            static readonly Type DTOType = typeof(TOutDTO);
            static readonly bool NeedToUseDTO = !DTOType.IsAssignableFrom(typeof(TModel));
            #endif  
            
            public ServiceTest()
            {
                Fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
                Contract = CreateService();
            } 
            
            protected abstract IContract<TOutDTO, TModel, TID> CreateService(); 
            /*
             *Usual test methods with [WithArgs] or [NoArgs] attributes.
            */ 
            
            #if (MODEL_USEDTO)
            protected TOutDTO? ToDTO(TModel? model)
            {
                if (model == null)
                    return default(TOutDTO);
                if (NeedToUseDTO)
                    return (TOutDTO)((IExModel)model).ToDTO(DTOType);
                return (TOutDTO)(object)model;
            }
            #else
            protected TOutDTO? ToDTO(TModel? model)
            {
                if(model == null)
                    return default(TOutDTO);
                return (TOutDTO)(object)model;
            }
            #endif   
      }

One for Standard Web API testing (Controller via Service repository)
    
    public abstract class TestStandard<TOutDTO, TModel, TID, TInDTO> 
        where TOutDTO : IModel, new()
        where TInDTO : IModel, new()
        where TModel : Model<TID, TModel>,
        #if (!MODEL_USEDTO)
        TOutDTO,
        #endif
        new()
        where TID : struct 
    {
        protected readonly Mock<IService<TOutDTO, TModel, TID>> MockService;
        readonly IContract<TOutDTO, TModel, TID> Contract;
        protected readonly List<TModel> Models;
        protected readonly IFixture Fixture;

        static readonly IExModelExceptionSupplier DummyModel = new TModel();
        #if MODEL_USEDTO
        static readonly Type DTOType = typeof(TOutDTO);
        static readonly bool NeedToUseDTO = !DTOType.IsAssignableFrom(typeof(TModel));
        #endif
        
        public TestStandard()
        {
            Fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
            MockService = Fixture.Freeze<Mock<IService<TOutDTO, TModel, TID>>>();
            Contract = CreateContract(MockService.Object);
            var count = DummyModelCount;
            if (count < 5)
                count = 5;
            Models = Fixture.CreateMany<TModel>(count).ToList();
        }
        
    #if (MODEL_USEDTO)
        protected IEnumerable<TOutDTO> Items => Models.Select(x => ToDTO(x));
    #else
        protected IEnumerable<TOutDTO> Items => (IEnumerable<TOutDTO>)Models;
    #endif

        protected virtual int DummyModelCount => 5;  
        
        protected abstract IContract<TOutDTO, TModel, TID> CreateContract(IContract<TOutDTO, TModel, TID> service); 
        
        protected void Setup<TResult>(Expression<Func<IService<TOutDTO, TModel, TID>, Task<TResult>>> expression, TResult returnedValue)
        {
            MockService.Setup(expression).ReturnsAsync(returnedValue);
        }
        protected void Setup<TResult>(Expression<Func<IService<TOutDTO, TModel, TID>, Task<TResult>>> expression, Exception exception)
        {
            MockService.Setup(expression).Throws(exception);
        } 
        
    #if (MODEL_USEDTO)
        protected TOutDTO? ToDTO(TModel? model)
        {
            if (model == null)
                return default(TOutDTO);
            if (NeedToUseDTO)
            {
                var result = ((IExModel)model).ToDTO(DTOType);
                if(result == null)
                    return default(TOutDTO);

                return (TOutDTO)result;
            }
            return (TOutDTO)(object)model;
        }
    #else
        protected TOutDTO? ToDTO(TModel? model)
        {
            if(model == null)
                return default(TOutDTO);
            return (TOutDTO)(object)model;
        }
    #endif 
    }

### These classes can be used commonly for all frameworks i.e. xUnit, NUnit or MSTest.

Which framework will be used can be decided by a user simply by defining compiler constants MODEL_USEXUNIT or MODEL_USENUNIT. 
If neither of those constants defined then MSTest will be used.
 
[GoTo Index](#Index)

## TASK2
Criteria based search TASK for models added.

Consider the following code:

    public abstract class Query<TOutDTO, TModel> : IQuery<TOutDTO, TModel> 
    where TModel : class, ISelfModel<TModel>, new()
    where TOutDTO : IModel, new() 
    {
         readonly static IExModel DummyModel = (IExModel)new TModel(); 
    #if MODEL_USEDTO
        static readonly Type DTOType = typeof(TOutDTO);
        static readonly bool NeedToUseDTO = !DTOType.IsAssignableFrom(typeof(TModel));
    #endif 

    #if (!MODEL_NONREADABLE || !MODEL_NONQUERYABLE) && MODEL_SEARCHABLE
        public virtual Task<IEnumerable<TOutDTO>?> FindAll<T>(AndOr conditionJoin, params T?[]? parameters)
            where T : ISearchParameter
        {
            if (parameters == null)
                throw DummyModel.GetModelException(ExceptionType.NoParameterSupplied);

            if (GetModelCount() == 0)
                throw DummyModel.GetModelException(ExceptionType.NoModelsFound);

            Predicate<TModel> predicate;

            if (parameters.Length == 1)
            {
                var parameter = parameters[0];

                if (parameter == null)
                    throw DummyModel.GetModelException(ExceptionType.NoParameterSupplied);

                if (string.IsNullOrEmpty(parameter.Name))
                    throw DummyModel.GetModelException(ExceptionType.NoParameterSupplied, "Missing Name!");

                predicate = (m) =>
                {
                    if (!DummyModel.Parse(parameter.Name, parameter.Value, out object? value, false, parameter.Criteria))
                        return false;

                    var currentValue = m[parameter.Name];
                    if (!Operations.Compare(currentValue, parameter.Criteria, value))
                        return false;

                    return true;
                };

                goto EXIT;
            }

            switch (conditionJoin)
            {
                case AndOr.AND:
                default:
                    predicate = (m) =>
                    {
                        foreach (var parameter in parameters)
                        {
                            if (parameter == null || string.IsNullOrEmpty(parameter.Name))
                                continue;
                            if (!DummyModel.Parse(parameter.Name, parameter.Value, out object? value, false, parameter.Criteria))
                                return false;

                            var currentValue = m[parameter.Name];
                            if (!Operations.Compare(currentValue, parameter.Criteria, value))
                                return false;
                        }
                        return true;
                    };
                    break;
                case AndOr.OR:
                    predicate = (m) =>
                    {
                        bool result = false;
                        foreach (var parameter in parameters)
                        {
                            if (parameter == null || string.IsNullOrEmpty(parameter.Name))
                                continue;
                            if (!DummyModel.Parse(parameter.Name, parameter.Value, out object? value, false, parameter.Criteria))
                                return false;

                            var currentValue = m[parameter.Name];
                            if (Operations.Compare(currentValue, parameter.Criteria, value))
                                return true;
                        }
                        return result;
                    };
                    break;
            }

            EXIT:
            return Task.FromResult(ToDTO(GetItems().Where((m) => predicate(m))));
        }
    #endif             
    }

Have a look at the Operations.cs class to know how generic comparison methods are defined.
 
[GoTo Index](#Index)

## TASK3
Support for ClassData and MemberData test attributes added.

ClassData attribute is mapped to: ArgSourceAttribute\<T\> where T: ArgSource
ArgSource is an abstract class with an abstract property IEnumerable<object[]> Data {get; }
You will have to inherit from this class and provide your own data and then you can use

This is an example on how to use source member data.
To use member data, you must define a static method or property returning IEnumerable<object[]>.

    [WithArgs]
    [ArgSource(typeof(MemberDataExample), "GetData")]
    public Task GetAll_ReturnAllUseMemberData(int limitOfResult = 0)
    {
        //
    }

This is an example on how to use source class data.
To use class data, ArgSource\<source\> will suffice.

    [WithArgs]
    [ArgSource<ClassDataExample>]
    public Task GetAll_ReturnAllUseClassData(int limitOfResult = 0)
    {
        //
    
    }
    
 Then, those classes can be defined in the following manner:
 
    class MemberDataExample 
    {
        public static IEnumerable<object[]> GetData   
        {
            get
            {
                yield return new object[] { 0 };
                yield return new object[] { 3 };
                yield return new object[] { -1 };
            }
        }
    }

    class ClassDataExample: ArgSource 
    {
        public override IEnumerable<object[]> Data  
        {
                get
                {
                    yield return new object[] { 0 };
                    yield return new object[] { 3 };
                    yield return new object[] { -1 };
                }
        }
    }

[GoTo Index](#Index)

## TASK4

Added Exception Middleware. Middleware type: IExceptionFiter type
First, out own exception class and exception type enum are needed:

    public class ModelException: Exception, IModelException
    {
        public ModelException(string message, ExceptionType type) :
            base(message)
        {
            Type = type;
        }
        public ModelException(ExceptionType type, Exception exception) :
            base(exception.Message, exception)
        {
            Type = type;
        }
        public ModelException(string message, ExceptionType type, Exception exception) :
           base(message, exception)
        {
            Type = type;
        }
        
        public ExceptionType Type { get; }

        public virtual int Status
        {
            get
            {
                switch (Type)
                {
                    case ExceptionType.Unknown:
                    case ExceptionType.NoModelFound:
                    case ExceptionType.NoModelFoundForID:
                    case ExceptionType.NoModelsFound:
                        return 404;
                    case Models.ExceptionType.NoModelSupplied:
                    case ExceptionType.NegativeFetchCount:
                    case ExceptionType.ModelCopyOperationFailed:
                    case ExceptionType.NoParameterSupplied:
                    case ExceptionType.NoParametersSupplied:
                    case ExceptionType.AddOperationFailed:
                    case ExceptionType.UpdateOperationFailed:
                    case ExceptionType.DeleteOperationFailed:
                        return 400;
                    case ExceptionType.InternalServerError:
                        return 500;
                    default:
                        return 400;
                }
            }
        }
        
        public void GetConsolidatedMessage(out string Title, out string Type, out string? Details, out string[]? stackTrace, bool isProductionEnvironment = false)
        {
            Title = Message;
            Type = this.Type.ToString();
            Details =  "None" ;
            stackTrace = new string[] { };

            if (InnerException != null)
            {
                Details = InnerException.Message;

                if (InnerException.StackTrace != null && !isProductionEnvironment)
                {
                    stackTrace = InnerException.StackTrace.Split(System.Environment.NewLine);
                }
            }
        }
    }
    
    public enum ExceptionType : ushort
    {
        Unknown = 0,

        /// <summary>
        /// Represents an exception to indicate that no model is found in the collection for a given search or the collection is empty.
        /// </summary>
        NoModelFound,

        /// <summary>
        /// Represents an exception to indicate that no model is found in the collection while searching it with specific ID.
        /// </summary>
        NoModelFoundForID,

        /*
           Other common exception types for models are provided.
           Add more types as per your need.
        */
    }

You can define more excetions types based on your needs.
Finally,

    public class HttpExceptionFilter : IExceptionFilter
    {
        void IExceptionFilter.OnException(ExceptionContext context)
        {
            ProblemDetails problem;

            if (context.Exception is IModelException)
            {
                var modelException = ((IModelException)context.Exception);                 

                modelException.GetConsolidatedMessage(out string title, out string type, out string? details, out string[]? stackTrace, Configuration.IsProductionEnvironment);
                problem = new ProblemDetails()
                {
                    Type = ((HttpStatusCode)modelException.Status).ToString() + ": " + type, // customize
                    Title = title, //customize
                    Status = modelException.Status, //customize
                    Detail = details,
                };
                var response = context.HttpContext.Response;
                response.ContentType = Contents.JSON;
                response.StatusCode = modelException.Status;
                if(stackTrace != null)
                {
                    response.WriteAsJsonAsync(new object[] { problem, stackTrace }).Wait();
                }
                else
                {
                    response.WriteAsJsonAsync(problem).Wait();
                }
                response.CompleteAsync().Wait();
                return;
            }
            
            problem = new ProblemDetails()
            {
                Type = "Error", // customize
                Title = "Error", //customize
                Status = (int)HttpStatusCode.ExpectationFailed, //customize
                Detail = context.Exception.Message,
            };
            context.Result = new JsonResult(problem);
        }
    }

[GoTo Index](#Index)

## TASK5
TASK: Choose database at model level.
    
    public enum ConnectionKey
    {
        InMemory,
    #if MODEL_CONNECTSQLSERVER        
        SQLServer = 1,
    #elif MODEL_CONNECTPOSTGRESQL
        PostgreSQL = 1,
    #elif MODEL_CONNECTMYSQL
        MySQL = 1,
    #endif
    }
    
Feel free to add more dabase options to the enum above if you desire so.
Create appropriate CCC for any option that you add. Idealy, there should
be only 2 options available: one is 'InMemory' and the other is for actual database.

To Use SQLServer:
1. define constant: MODEL_CONNECTSQLSERVER
2. In configuration, define connection string with key "SQLServer".
3. At your model level, use model attribute with ConnectionKey = ConnectionKey.SQLServer

To Use PostgreSQL:
1. define constant: MODEL_CONNECTPOSTGRESQL
2. In configuration, define connection string with key "PostgreSQL".
3. At your model level, use model attribute with ConnectionKey = ConnectionKey.PostgreSQL

To Use MySQL:
1. define constant: MODEL_CONNECTMYSQL
2. In configuration, define connection string with key "MySQL".
3. At your model level, use model attribute with ConnectionKey = ConnectionKey.MySQL

Please note that, regardless of any of these,
1. if connectionstring is empty, InMemory SQLLite will be used as default.
2. Don't worry about downloading relevant package from nuget.
3. Defining constant will automatically download the relevant package for you.

## TASK6
Defining API End-Points. 
Consider the following code in partial Command service class:

        partial class CommandService<TOutDTO, TModel, TID, TInDTO, TContext> : IExContract
        {
            protected override void Configure(IEndpointRouteBuilder app)
            {
        #if MODEL_APPENDABLE
                app.MapPost(GetUrl("Add"), [Tags("Command")] async(Parser<TInDTO?> model) =>
                await Command.Add(model.Result));
        #endif
        #if MODEL_DELETABLE
                app.MapDelete(GetUrl("Delete/{id}"), [Tags("Command")] async (TID id) => 
                await Command.Delete(id));
        #endif                  
        #if MODEL_UPDATABLE
                app.MapPut(GetUrl("Update/{id},{model}"), [Tags("Command")] async (TID id, [FromBody] TInDTO? model) => 
                await Command.Update(id, model));
        #endif
        #if (MODEL_APPENDABLE) && MODEL_APPENDBULK
                app.MapPost(GetUrl("AddBulk"), [Tags("Command")] async (Parser<IEnumerable<TInDTO?>?> models) =>
                await Command.AddRange(models.Result));
        #endif
        #if (MODEL_UPDATABLE) && MODEL_UPDATEBULK
                app.MapPost(GetUrl("UpdateBulk"), [Tags("Command")] async([FromQuery] Parser< IEnumerable<TID>> IDs, Parser< IEnumerable<TInDTO?>?> models) =>
                await Command.UpdateRange(IDs.Result, models.Result));
        #endif
        #if (MODEL_DELETABLE) && MODEL_DELETEBULK
                app.MapPost(GetUrl("DeleteBulk"), [Tags("Command")] async([FromQuery] Parser< IEnumerable<TID>> IDs) =>
                await Command.DeleteRange(IDs.Result));
        #endif
            }
        }
 
[GoTo Index](#Index)

## TASK7
Converted DBContext from generic to non-generic class.
This is to allow single DBContext to hold multiple model sets..

    public partial class DBContext : DbContext, IModelContext
    {
        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder = modelBuilder.ApplyConfigurationsFromAssembly(typeof(DBContext).Assembly);
        }
        
        #if MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE
        ICommand<TOutDTO, TModel, TID> IModelContext.CreateCommand<TOutDTO, TModel, TID>(bool initialzeData, ICollection<TModel>? source)
        {        
            return new CommandObject<TOutDTO, TModel, TID>(this, source, initialzeData);
        }
        #endif
        
        #if !MODEL_NONREADABLE || !MODEL_NONQUERYABLE
        IQuery<TOutDTO, TModel> IModelContext.CreateQuery<TOutDTO, TModel>(bool initialzeData, ICollection<TModel>? source)
        {
            return new QueryObject<TOutDTO, TModel>(this, null, source, initialzeData);
        }
        IQuery<TOutDTO, TModel, TID> IModelContext.CreateQuery<TOutDTO, TModel, TID>(bool initialzeData, ICollection<TModel>? source)
        {
            return new QueryObject<TOutDTO, TModel, TID>(this, null, source, initialzeData);
        }
        #endif
    }

OnModelCreating(ModelBuilder modelBuilder) method is the most important method, 
beacause we are adding DbSets dynamically, we must make sure that our DBContext recognises them
and does not throw an error. To achieve that, we already created partial Model\<TModel\> class and implemented
ISelfModel\<TModel\> interface so, another partial declaration in Web-API/Models folder:
Learned it from here: https://learn.microsoft.com/en-us/ef/core/modeling/ and then I had to make breaking changes
all the way upto the model class and interfaces to define Model\<TModel\> and ISelfModel\<TModel\> 

    partial class Model<TModel> : IEntityTypeConfiguration<TModel>, IExModel
    {
        protected virtual void Configure(EntityTypeBuilder<TModel> builder) { }
        void IEntityTypeConfiguration<TModel>.Configure(EntityTypeBuilder<TModel> builder)
        {
            Configure(builder);            
        }
    }
IEntityTypeConfiguration\<TModel\> is the key. Now every model that inherits from Model\<TModel\>
will not need to worry about getting associated with DBContext.

[GoTo Index](#Index)

## TASK8 
Support for Query-Only-Controllers and Keyless models is added.

    [Keyless]
    public abstract class KeylessModel<TModel>: Model<TModel>, IEntityTypeConfiguration<TModel>
        where TModel : Model<TModel>
    {
        int PsuedoID;

        void IEntityTypeConfiguration<TModel>.Configure(EntityTypeBuilder<TModel> builder)
        {
            base.Configure(builder);
            builder.HasKey("PsuedoID");
        }
    }

It is now possible to create separate controller for command and query purposes.

Use constant MODEL_NONREADABLE: this will create Command-only controller.
Then for the same model, call AddQueryModel() method, which is located in Configuration class, will create Query-only controller.

[GoTo Index](#Index)

## TASK9
Abstract Models for common primary key type: int, long, Guid, enum are added.

    public abstract class ModelEnum<TEnum, TModel> : Model<TEnum, TModel>
        where TModel : ModelEnum<TEnum, TModel>
        where TEnum : struct, Enum
    {
        #region VARIABLES
        static HashSet<TEnum> Used  = new HashSet<TEnum>();
        static List<TEnum> Available = new List<TEnum>();
        static volatile int index;
        #endregion

        #region CONSTRUCTORS
        static ModelEnum()
        {
            Available = new List<TEnum>(Enum.GetValues<TEnum>());
        }
        protected ModelEnum() :
            base(false)
        { }
        protected ModelEnum(bool generateNewID) :
            base(generateNewID)
        { }
        #endregion

        #region GET NEW ID
        protected override TEnum GetNewID()
        {
           if(Available.Count == 0)
                return default(TEnum);
            var newID = Available[index];
            if (!Used.Contains(newID))
            {
                Used.Add(newID);
                return newID;
            }
            while (index < Available.Count)
            {
                newID = Available[index++];

                if (!Used.Contains(newID))
                {
                    Used.Add(newID);
                    return newID;
                }
            }
            return default(TEnum);
        }
        #endregion

        #region TRY PARSE ID
        protected override bool TryParseID(object value, out TEnum newID)
        {
            return Enum.TryParse(value.ToString(), true, out newID);
        }
        #endregion
    }
    
    public abstract class ModelInt32<TModel> : Model<int, TModel>
        where TModel : ModelInt32<TModel>
    {
        #region VARIABLES
        static volatile int IDCounter;
        #endregion

        #region CONSTRUCTORS
        protected ModelInt32() :
            base(false)
        { }
        protected ModelInt32(bool generateNewID) :
            base(generateNewID)
        { }
        #endregion

        #region GET NEW ID
        protected override int GetNewID()
        {
            return ++IDCounter;
        }
        #endregion

        #region TRY PARSE ID
        protected override bool TryParseID(object value, out int newID)
        {
            return int.TryParse(value.ToString(), out newID);
        }
        #endregion
    }
In similiar fashion ModelInt64\<TModel\> and ModelGuid\<TModel\> classes are created.
If you want string keyed model, you will need to create a struct which contains string value
and use as 'TID' because TID can only be struct. 
Also note that when you are using an actual database GetNewID() method implementation might change;
You may want to get unique ID from the database itself.    

[GoTo Index](#Index)

## TASK10
 
Added: Support for List based (non DbContext) Sigleton CQRS
Changes are made in IModelContext, Service classes and Configuration class 
to add a singleton service by passing List\<TModel\> instance for command and query both.
Consider the following modified definition of IModelContext interface:

    public interface IModelContext : IDisposable
    {
    #if MODEL_APPENDABLE || MODEL_UPDATABLE || MODEL_DELETABLE
        ICommand<TOutDTO, TModel, TID> CreateCommand<TOutDTO, TModel, TID>(bool initialzeData = true, ICollection<TModel>? source = null)
            where TOutDTO : IModel, new()
            where TModel : class, ISelfModel<TID, TModel>, new()
        #if (!MODEL_USEDTO)
            , TOutDTO
        #endif
        where TID : struct
        ;
    #endif

    #if !MODEL_NONREADABLE || !MODEL_NONQUERYABLE        
        IQuery<TOutDTO, TModel> CreateQuery<TOutDTO, TModel>(bool initialzeData = false, ICollection<TModel>? source = null)
            where TModel : class, ISelfModel<TModel>, new()
            where TOutDTO : IModel, new()
            ;
        IQuery<TOutDTO, TModel, TID> CreateQuery<TOutDTO, TModel, TID>(bool initialzeData = false, ICollection<TModel>? source = null)
            #region TYPE CONSTRINTS
            where TOutDTO : IModel, new()
            where TModel : class, ISelfModel<TID, TModel>, new()
        #if (!MODEL_USEDTO)
            , TOutDTO
        #endif
        where TID : struct
        ;
    #endif
    }
As you can see, external source can be passed while creating command or query object.

[GoTo Index](#Index)

## What_Next

I will soon add event sourcing, docker and kubernetes support.
