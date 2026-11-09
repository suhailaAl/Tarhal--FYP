using Tarhal_FYP.Models;
using Microsoft.ML.Data;
using Microsoft.ML;
using Microsoft.EntityFrameworkCore;

public class Recommendation_System
{
	// Global varibles 
	TarhalDbContext db = new TarhalDbContext();
	MLContext mlContext = new MLContext();

	// Calss to store the score of the preidection 
	public class RecommendationPrediction
	{
		[ColumnName("Score")]
		public float Score { get; set; }
	}

	// Generate activities recommendations 
	public List<int> recommend_Activities(Plan plan)
	{

		// Historical implicit feedback data related to tourists and their selected activities
		var allData = db.PlanActivityViews
		.Select(v => new ActivityTouristInteraction
		{
			Budget = v.Budget ?? "$$",
			Preferences = v.TouristPreferences ?? "History",
			TripType = v.TripType ?? "Family",
			Travel_time = (float)(v.TravelTime ?? 0),
			Duration = v.Duration ?? 0,
			Activity16 = v.Activity16 == 0 ? false : true,
			Activity17 = v.Activity17 == 0 ? false : true,
			Activity18 = v.Activity18 == 0 ? false : true,
			Activity19 = v.Activity18 != 0 ? false : true,
			Activity20 = v.Activity20 == 0 ? false : true,
			Activity21 = v.Activity21 == 0 ? false : true,
			Activity22 = v.Activity22 == 0 ? false : true,
			Activity23 = v.Activity23 == 0 ? false : true,
			Activity24 = v.Activity24 == 0 ? false : true,
			Activity25 = v.Activity25 == 0 ? false : true,
			Activity26 = v.Activity26 == 0 ? false : true,
			Activity27 = v.Activity27 == 0 ? false : true,
			Activity28 = v.Activity28 == 0 ? false : true,
			Activity29 = v.Activity29 == 0 ? false : true,
			Activity30 = v.Activity30 == 0 ? false : true
		}).ToList();

		// Cleaning the dataset

		// Remove the duplicates
		var distinctData = allData.DistinctBy(x => new
		{
			x.Duration,
			x.TripType,
			x.Travel_time,
			x.Budget,
			x.Preferences,
			x.Activity16,
			x.Activity17,
			x.Activity18,
			x.Activity19,
			x.Activity20,
			x.Activity21,
			x.Activity22,
			x.Activity23,
			x.Activity24,
			x.Activity25,
			x.Activity26,
			x.Activity27,
			x.Activity28,
			x.Activity29,
			x.Activity30
		}).ToList();

		string[] activityColumns = new[]
		{
		"Activity16", "Activity17", "Activity18", "Activity19", "Activity20",
		"Activity21", "Activity22", "Activity23", "Activity24", "Activity25",
		"Activity26", "Activity27", "Activity28", "Activity29", "Activity30"
		};

		// Undersample for all activities that has imbalance 
		var balancedTrainingData = new List<ActivityTouristInteraction>();

		foreach (var activity in activityColumns)
		{
			var positiveSamples = distinctData.Where(row => (bool)row.GetType().GetProperty(activity).GetValue(row)).ToList();
			var negativeSamples = distinctData.Where(row => !(bool)row.GetType().GetProperty(activity).GetValue(row)).ToList();

			if (positiveSamples.Count > negativeSamples.Count)
			{
				var undersampledPositives = positiveSamples.Take(negativeSamples.Count).ToList();
				balancedTrainingData.AddRange(undersampledPositives);
				balancedTrainingData.AddRange(negativeSamples);
			}
			else
			{
				var undersampledNegatives = negativeSamples.Take(positiveSamples.Count).ToList();
				balancedTrainingData.AddRange(positiveSamples);
				balancedTrainingData.AddRange(undersampledNegatives);
			}
		}

		// Load data into IDataView 
		var balancedDataView = mlContext.Data.LoadFromEnumerable(balancedTrainingData);

		// Preprocessing pipeline and build the modle with SdcaLogisticRegression algo for each attraction lable
		foreach (var activity in activityColumns)
		{
			Console.WriteLine($"Training model for {activity}...");
			var pipeline = mlContext.Transforms.Categorical.OneHotEncoding("PreferredActivityEncoded", nameof(ActivityTouristInteraction.Preferences))
			.Append(mlContext.Transforms.Categorical.OneHotEncoding("TripTypeEncoded", nameof(ActivityTouristInteraction.TripType)))
			.Append(mlContext.Transforms.Categorical.OneHotEncoding("BudgetEncoded", nameof(ActivityTouristInteraction.Budget)))
			.Append(mlContext.Transforms.NormalizeMeanVariance(nameof(ActivityTouristInteraction.Travel_time)))
			.Append(mlContext.Transforms.Conversion.ConvertType(nameof(ActivityTouristInteraction.Duration), outputKind: DataKind.Single)) // Convert Duration to float
			.Append(mlContext.Transforms.NormalizeMeanVariance(nameof(ActivityTouristInteraction.Duration))) // Normalize Duration
			.Append(mlContext.Transforms.Concatenate("Features",
			"PreferredActivityEncoded",
			"TripTypeEncoded",
			"BudgetEncoded",
			nameof(ActivityTouristInteraction.Travel_time),
			nameof(ActivityTouristInteraction.Duration))
			.Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
			labelColumnName: activity,
			featureColumnName: "Features"

			)));

			var model = pipeline.Fit(balancedDataView);

			// Save the model for future predictions
			mlContext.Model.Save(model, balancedDataView.Schema, $"{activity}_Model.zip");

		}


		// Predict recommended activities for a new plan

		// Store the new plan data 
		var newTouristInteraction = new ActivityTouristInteraction
		{
			Duration = (plan.DateEnd.DayNumber - plan.DateBegin.DayNumber) + 1,
			TripType = plan.TripType ?? "Family",
			Travel_time = (float)(plan.TravelTime ?? 10),
			Budget = "$$",
			Preferences = plan.Preferences
		};

		// Predict and rank activities
		var selectedActivityIds = new List<int>();

		foreach (var activity in activityColumns)
		{
			// Load the model for the current attraction
			var model = mlContext.Model.Load($"{activity}_Model.zip", out var inputSchema);
			var predictionEngine = mlContext.Model.CreatePredictionEngine<ActivityTouristInteraction, RecommendationPrediction>(model);

			// Predict the score for the current attraction
			var prediction = predictionEngine.Predict(newTouristInteraction);
			var activityId = int.Parse(activity.Replace("Activity", ""));
			var rate = (float)db.Activities.Where(ac => ac.ActivityId == activityId)
						.Select(ac => ac.Rate).First();

			// Add to the list only if the score meets the threshold
			if (prediction.Score >= 0.6) // threshold 0.6
			{
				selectedActivityIds.Add(activityId);
			}
		}

		// Determine the number of recommendations based on the duration
		int recommendationsNum = newTouristInteraction.Duration * 2;

		// Retrieve all activities from the database
		var allActivities = db.Activities.ToList();

		// Separate selected and non-selected activities
		var selectedActivities = allActivities
			.Where(ac => selectedActivityIds.Contains(ac.ActivityId))
			.OrderByDescending(ac => ac.Rate) // Order by rate descending
			.ToList();

		var nonSelectedActivities = allActivities
			.Where(ac => !selectedActivityIds.Contains(ac.ActivityId))
			.OrderByDescending(ac => ac.Rate)
			.ToList();


		// Combine selected and non-selected to complete the required number of recommendations
		var finalActivities = new List<Activity>();
		if (recommendationsNum >= 14)
		{
			finalActivities = selectedActivities
		   .Concat(nonSelectedActivities)
		   .OrderByDescending(ac => ac.Rate)
		   //.Select (x => x.ActivityId)
		   .ToList();
		}
		else
		{
			finalActivities = selectedActivities
		   .Concat(nonSelectedActivities)
		   .OrderByDescending(ac => ac.Rate)
		   .Take(recommendationsNum)
		   //.Select (x => x.ActivityId)
		   .ToList();

		}

		// Display the results
		Console.WriteLine("Selected Activities Based on Rate:");
		foreach (var activity in finalActivities)
		{
			Console.WriteLine($"Activity ID: {activity.ActivityId}, Rate: {activity.Rate:F2}");
		}

		Console.WriteLine("Selected Activity IDs:");
		Console.WriteLine(string.Join(", ", finalActivities.Select(ac => ac.ActivityId)));

		var finalActivitiesIds = finalActivities.Select(ac => ac.ActivityId).ToList();
		return finalActivitiesIds;
	}

	// Generate attractions recommendations 
	public List<int> recommend_Attractions(Plan plan)
	{

		// Historical implicit feedback data related to tourists and their selected attractions
		var allData = db.PlanAttractionViews
		 .Select(v => new AttractionTouristInteraction
		 {
			 Budget = v.Budget ?? "$$",
			 Preferences = v.TouristPreferences ?? "History,Nature",
			 TripType = v.TripType ?? "Family",
			 Travel_time = (float)(v.TravelTime ?? 0),
			 Duration = v.Duration ?? 0,
			 Attraction1 = v.Attraction1 == 0 ? false : true,
			 Attraction2 = v.Attraction2 == 0 ? false : true,
			 Attraction3 = v.Attraction3 == 0 ? false : true,
			 Attraction4 = v.Attraction4 != 0 ? false : true,
			 Attraction5 = v.Attraction5 == 0 ? false : true,
			 Attraction6 = v.Attraction6 == 0 ? false : true,
			 Attraction7 = v.Attraction7 == 0 ? false : true,
			 Attraction8 = v.Attraction8 == 0 ? false : true,
			 Attraction9 = v.Attraction9 == 0 ? false : true,
			 Attraction10 = v.Attraction10 == 0 ? false : true,
			 Attraction11 = v.Attraction11 == 0 ? false : true,
			 Attraction12 = v.Attraction12 == 0 ? false : true,
			 Attraction13 = v.Attraction13 == 0 ? false : true,
			 Attraction14 = v.Attraction14 == 0 ? false : true,
			 Attraction15 = v.Attraction15 == 0 ? false : true
		 }).ToList();

		// Cleaning the dataset

		// Remove the duplicates
		var distinctData = allData.DistinctBy(x => new
		{
			x.Duration,
			x.TripType,
			x.Travel_time,
			x.Budget,
			x.Preferences,
			x.Attraction1,
			x.Attraction2,
			x.Attraction3,
			x.Attraction4,
			x.Attraction5,
			x.Attraction6,
			x.Attraction7,
			x.Attraction8,
			x.Attraction9,
			x.Attraction10,
			x.Attraction11,
			x.Attraction12,
			x.Attraction13,
			x.Attraction14,
			x.Attraction15
		}).ToList();

		string[] attractionColumns = new[]
		{
		"Attraction1", "Attraction2", "Attraction3", "Attraction4", "Attraction5",
		"Attraction6", "Attraction7", "Attraction8", "Attraction9", "Attraction10",
		"Attraction11", "Attraction12", "Attraction13", "Attraction14", "Attraction15"
		};

		// Undersample for all activities that has imbalance 
		var balancedTrainingData = new List<AttractionTouristInteraction>();

		foreach (var attraction in attractionColumns)
		{
			var positiveSamples = distinctData.Where(row => (bool)row.GetType().GetProperty(attraction).GetValue(row)).ToList();
			var negativeSamples = distinctData.Where(row => !(bool)row.GetType().GetProperty(attraction).GetValue(row)).ToList();

			if (positiveSamples.Count > negativeSamples.Count)
			{
				var undersampledPositives = positiveSamples.Take(negativeSamples.Count).ToList();
				balancedTrainingData.AddRange(undersampledPositives);
				balancedTrainingData.AddRange(negativeSamples);
			}
			else
			{
				var undersampledNegatives = negativeSamples.Take(positiveSamples.Count).ToList();
				balancedTrainingData.AddRange(positiveSamples);
				balancedTrainingData.AddRange(undersampledNegatives);
			}
		}

		// Load data into IDataView 
		var balancedDataView = mlContext.Data.LoadFromEnumerable(balancedTrainingData);

		// Preprocessing pipeline and build the modle with SdcaLogisticRegression algo for each attraction lable
		foreach (var attraction in attractionColumns)
		{
			var pipeline = mlContext.Transforms.Categorical.OneHotEncoding("PreferredActivityEncoded", nameof(ActivityTouristInteraction.Preferences))
			.Append(mlContext.Transforms.Categorical.OneHotEncoding("TripTypeEncoded", nameof(ActivityTouristInteraction.TripType)))
			.Append(mlContext.Transforms.Categorical.OneHotEncoding("BudgetEncoded", nameof(ActivityTouristInteraction.Budget)))
			.Append(mlContext.Transforms.NormalizeMeanVariance(nameof(ActivityTouristInteraction.Travel_time)))
			.Append(mlContext.Transforms.Conversion.ConvertType(nameof(ActivityTouristInteraction.Duration), outputKind: DataKind.Single)) // Convert Duration to float
			.Append(mlContext.Transforms.NormalizeMeanVariance(nameof(ActivityTouristInteraction.Duration))) // Normalize Duration
			.Append(mlContext.Transforms.Concatenate("Features",
			"PreferredActivityEncoded",
			"TripTypeEncoded",
			"BudgetEncoded",
			nameof(ActivityTouristInteraction.Travel_time),
			nameof(ActivityTouristInteraction.Duration))
			.Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
			labelColumnName: attraction,
			featureColumnName: "Features"

			)));

			var model = pipeline.Fit(balancedDataView);

			// Save the model for future predictions
			mlContext.Model.Save(model, balancedDataView.Schema, $"{attraction}_Model.zip");

		}


		// Predict recommended attractions for a new plan

		// Store the new plan data 
		var newTouristInteraction = new AttractionTouristInteraction
		{
			Duration = (plan.DateEnd.DayNumber - plan.DateBegin.DayNumber) + 1,
			TripType = plan.TripType ?? "Family",
			Travel_time = (float)(plan.TravelTime ?? 10),
			Budget = "$$",
			Preferences = plan.Preferences
		};

		// Predict and rank attractions
		var selectedActivityIds = new List<int>();

		foreach (var attraction in attractionColumns)
		{
			// Load the model for the current attraction
			var model = mlContext.Model.Load($"{attraction}_Model.zip", out var inputSchema);
			var predictionEngine = mlContext.Model.CreatePredictionEngine<AttractionTouristInteraction, RecommendationPrediction>(model);

			// Predict the score for the current attraction
			var prediction = predictionEngine.Predict(newTouristInteraction);
			var attractionId = int.Parse(attraction.Replace("Attraction", ""));

			// Add to the list only if the score meets the threshold
			if (prediction.Score >= 0.6) // threshold 0.6
			{
				selectedActivityIds.Add(attractionId);
			}
		}

		// Determine the number of recommendations based on the duration
		int recommendationsNum = newTouristInteraction.Duration * 2;

		// Retrieve all attractions from the database
		var allAttractions = db.Attractions.ToList();

		// Separate selected and non-selected attractions
		var selectedAttractions = allAttractions
			.Where(ac => selectedActivityIds.Contains(ac.AttractionId))
			.OrderByDescending(ac => ac.Rate) // Order by rate descending
			.ToList();

		var nonSelectedAttractions = allAttractions
			.Where(ac => !selectedActivityIds.Contains(ac.AttractionId))
			.OrderByDescending(ac => ac.Rate)
			.ToList();


		// Combine selected and non-selected to complete the required number of recommendations
		var finalAttractions = new List<int>();
		if (recommendationsNum >= 14)
		{
			finalAttractions = selectedAttractions
		   .Concat(nonSelectedAttractions)
		   .OrderByDescending(ac => ac.Rate)
		   .Select(ac => ac.AttractionId)
		   .ToList();
		}
		else
		{
			finalAttractions = selectedAttractions
		   .Concat(nonSelectedAttractions)
		   .OrderByDescending(ac => ac.Rate)
		   .Take(recommendationsNum)
		   .Select(c => c.AttractionId)
		   .ToList();

		}

		return finalAttractions;
	}

	// Generate accommodations recommendations 
	public List<int> recommend_Accommodations(Plan plan)
	{

		// Historical implicit feedback data related to tourists and their selected accommodations
		var allData = db.PlanAccommodationViews
		 .Select(v => new AccommodationTouristInteraction
		 {
			 Budget = v.Budget ?? "$$",
			 TripType = v.TripType ?? "Family",
			 Travel_time = (float)(v.TravelTime ?? 0),
			 Duration = v.Duration ?? 0,
			 Accommodation31 = v.Accommodation31 == 0 ? false : true,
			 Accommodation32 = v.Accommodation32 == 0 ? false : true,
			 Accommodation33 = v.Accommodation33 == 0 ? false : true,
			 Accommodation34 = v.Accommodation34 != 0 ? false : true,
			 Accommodation35 = v.Accommodation35 == 0 ? false : true,
			 Accommodation36 = v.Accommodation36 == 0 ? false : true,
			 Accommodation37 = v.Accommodation37 == 0 ? false : true,
			 Accommodation38 = v.Accommodation38 == 0 ? false : true,
			 Accommodation39 = v.Accommodation39 == 0 ? false : true,
			 Accommodation40 = v.Accommodation40 == 0 ? false : true,
			 Accommodation41 = v.Accommodation41 == 0 ? false : true,
			 Accommodation42 = v.Accommodation42 == 0 ? false : true,
			 Accommodation43 = v.Accommodation43 == 0 ? false : true,
			 Accommodation44 = v.Accommodation44 == 0 ? false : true,
			 Accommodation45 = v.Accommodation45 == 0 ? false : true
		 }).ToList();

		// Cleaning the dataset

		// Remove the duplicates
		var distinctData = allData.DistinctBy(x => new
		{
			x.Duration,
			x.TripType,
			x.Travel_time,
			x.Budget,
			x.Accommodation31,
			x.Accommodation32,
			x.Accommodation33,
			x.Accommodation34,
			x.Accommodation35,
			x.Accommodation36,
			x.Accommodation37,
			x.Accommodation38,
			x.Accommodation39,
			x.Accommodation40,
			x.Accommodation41,
			x.Accommodation42,
			x.Accommodation43,
			x.Accommodation44,
			x.Accommodation45
		}).ToList();

		string[] accommodationColumns = new[]
		{
		"Accommodation31", "Accommodation32", "Accommodation33", "Accommodation34", "Accommodation35",
		"Accommodation36", "Accommodation37", "Accommodation38", "Accommodation39", "Accommodation40",
		"Accommodation41", "Accommodation42", "Accommodation43", "Accommodation44", "Accommodation45"
		};

		// check the balance of the lables
		foreach (var accommodation in accommodationColumns)
		{
			var positiveSamples = distinctData.Count(row => (bool)row.GetType().GetProperty(accommodation).GetValue(row));
			var negativeSamples = distinctData.Count(row => !(bool)row.GetType().GetProperty(accommodation).GetValue(row));
		}

		// Undersample for all accommodations that has imbalance 
		var balancedTrainingData = new List<AccommodationTouristInteraction>();

		foreach (var accommodation in accommodationColumns)
		{
			var positiveSamples = distinctData.Where(row => (bool)row.GetType().GetProperty(accommodation).GetValue(row)).ToList();
			var negativeSamples = distinctData.Where(row => !(bool)row.GetType().GetProperty(accommodation).GetValue(row)).ToList();

			if (positiveSamples.Count > negativeSamples.Count)
			{
				var undersampledPositives = positiveSamples.Take(negativeSamples.Count).ToList();
				balancedTrainingData.AddRange(undersampledPositives);
				balancedTrainingData.AddRange(negativeSamples);
			}
			else
			{
				var undersampledNegatives = negativeSamples.Take(positiveSamples.Count).ToList();
				balancedTrainingData.AddRange(positiveSamples);
				balancedTrainingData.AddRange(undersampledNegatives);
			}
		}

		// Load data into IDataView 
		var balancedDataView = mlContext.Data.LoadFromEnumerable(balancedTrainingData);

		// Preprocessing pipeline and build the modle with SdcaLogisticRegression algo for each accommodation lable
		foreach (var accommodation in accommodationColumns)
		{
			var pipeline = mlContext.Transforms.Categorical.OneHotEncoding("TripTypeEncoded", nameof(ActivityTouristInteraction.TripType))
			.Append(mlContext.Transforms.Categorical.OneHotEncoding("BudgetEncoded", nameof(ActivityTouristInteraction.Budget)))
			.Append(mlContext.Transforms.NormalizeMeanVariance(nameof(ActivityTouristInteraction.Travel_time)))
			.Append(mlContext.Transforms.Conversion.ConvertType(nameof(ActivityTouristInteraction.Duration), outputKind: DataKind.Single)) // Convert Duration to float
			.Append(mlContext.Transforms.NormalizeMeanVariance(nameof(ActivityTouristInteraction.Duration))) // Normalize Duration
			.Append(mlContext.Transforms.Concatenate("Features",
			"TripTypeEncoded",
			"BudgetEncoded",
			nameof(ActivityTouristInteraction.Travel_time),
			nameof(ActivityTouristInteraction.Duration))
			.Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
			labelColumnName: accommodation,
			featureColumnName: "Features"

			)));

			var model = pipeline.Fit(balancedDataView);

			// Save the model for future predictions
			mlContext.Model.Save(model, balancedDataView.Schema, $"{accommodation}_Model.zip");

		}


		// Predict recommended accommodations for a new plan

		// Store the new plan data 
		var newTouristInteraction = new AccommodationTouristInteraction
		{
			Duration = (plan.DateEnd.DayNumber - plan.DateBegin.DayNumber) + 1,
			TripType = plan.TripType ?? "Family",
			Travel_time = (float)(plan.TravelTime ?? 10),
			Budget = "$$",
		};

		// Predict and rank accommodations
		var selectedAccommodationIds = new List<int>();

		foreach (var accommodation in accommodationColumns)
		{
			// Load the model for the current accommodation
			var model = mlContext.Model.Load($"{accommodation}_Model.zip", out var inputSchema);
			var predictionEngine = mlContext.Model.CreatePredictionEngine<AccommodationTouristInteraction, RecommendationPrediction>(model);

			// Predict the score for the current accommodation
			var prediction = predictionEngine.Predict(newTouristInteraction);
			var accommodationId = int.Parse(accommodation.Replace("Accommodation", ""));

			// Add to the list only if the score meets the threshold
			if (prediction.Score >= 0.6) // threshold 0.6
			{
				selectedAccommodationIds.Add(accommodationId);
			}
		}

		int recommendationsNum = (newTouristInteraction.Duration / 2) + 2;

		// Retrieve all accommodations from the database
		var allAccommodationId = db.Accommodations.ToList();

		// Separate selected and non-selected accommodations
		var selectedAccommodations = allAccommodationId
			.Where(x => selectedAccommodationIds.Contains(x.AccommodationId))
			.OrderByDescending(x => x.Rate) // Order by rate descending
			.ToList();

		var nonSelectedAccommodations = allAccommodationId
			.Where(x => !selectedAccommodationIds.Contains(x.AccommodationId))
			.OrderByDescending(x => x.Rate)
			.ToList();


		// Combine selected and non-selected to complete the required number of recommendations
		var finalAccommodations = new List<int>();
		if (recommendationsNum >= 14)
		{
			finalAccommodations = selectedAccommodations
		   .Concat(nonSelectedAccommodations)
		   .OrderByDescending(ac => ac.Rate)
		   .Select(ac => ac.AccommodationId)
		   .ToList();
		}
		else
		{
			finalAccommodations = selectedAccommodations
		   .Concat(nonSelectedAccommodations)
		   .OrderByDescending(ac => ac.Rate)
		   .Take(recommendationsNum)
		   .Select(c => c.AccommodationId)
		   .ToList();

		}

		return finalAccommodations;
	}

	// Generate restaurants recommendations 
	public List<int> recommend_Restaurants(Plan plan)
	{

		// Historical implicit feedback data related to tourists and their selected restaurants
		var allData = db.PlanRestaurantViews
		 .Select(v => new RestaurantTouristInteraction
		 {
			 Budget = v.Budget ?? "$$",
			 TripType = v.TripType ?? "Family",
			 Travel_time = (float)(v.TravelTime ?? 0),
			 Duration = v.Duration ?? 0,
			 Restaurant46 = v.Restaurant46 == 0 ? false : true,
			 Restaurant47 = v.Restaurant47 == 0 ? false : true,
			 Restaurant48 = v.Restaurant48 == 0 ? false : true,
			 Restaurant49 = v.Restaurant49 != 0 ? false : true,
			 Restaurant50 = v.Restaurant50 == 0 ? false : true,
			 Restaurant51 = v.Restaurant51 == 0 ? false : true,
			 Restaurant52 = v.Restaurant52 == 0 ? false : true,
			 Restaurant53 = v.Restaurant53 == 0 ? false : true,
			 Restaurant54 = v.Restaurant54 == 0 ? false : true,
			 Restaurant55 = v.Restaurant55 == 0 ? false : true,
			 Restaurant56 = v.Restaurant56 == 0 ? false : true,
			 Restaurant57 = v.Restaurant57 == 0 ? false : true,
			 Restaurant58 = v.Restaurant58 == 0 ? false : true,
			 Restaurant59 = v.Restaurant59 == 0 ? false : true

		 }).ToList();

		// Cleaning the dataset

		// Remove the duplicates
		var distinctData = allData.DistinctBy(x => new
		{
			x.Duration,
			x.TripType,
			x.Travel_time,
			x.Budget,
			x.Restaurant46,
			x.Restaurant47,
			x.Restaurant48,
			x.Restaurant49,
			x.Restaurant50,
			x.Restaurant51,
			x.Restaurant52,
			x.Restaurant53,
			x.Restaurant54,
			x.Restaurant55,
			x.Restaurant56,
			x.Restaurant57,
			x.Restaurant58,
			x.Restaurant59

		}).ToList();

		string[] restaurantColumns = new[]
		{
		"Restaurant46", "Restaurant47", "Restaurant48", "Restaurant49", "Restaurant50",
		"Restaurant51", "Restaurant52", "Restaurant53", "Restaurant54", "Restaurant55",
		"Restaurant56", "Restaurant57", "Restaurant58", "Restaurant59"
		};

		// Undersample for all restaurants that has imbalance 
		var balancedTrainingData = new List<RestaurantTouristInteraction>();

		foreach (var restaurant in restaurantColumns)
		{
			var positiveSamples = distinctData.Where(row => (bool)row.GetType().GetProperty(restaurant).GetValue(row)).ToList();
			var negativeSamples = distinctData.Where(row => !(bool)row.GetType().GetProperty(restaurant).GetValue(row)).ToList();

			if (positiveSamples.Count > negativeSamples.Count)
			{
				var undersampledPositives = positiveSamples.Take(negativeSamples.Count).ToList();
				balancedTrainingData.AddRange(undersampledPositives);
				balancedTrainingData.AddRange(negativeSamples);
			}
			else
			{
				var undersampledNegatives = negativeSamples.Take(positiveSamples.Count).ToList();
				balancedTrainingData.AddRange(positiveSamples);
				balancedTrainingData.AddRange(undersampledNegatives);
			}
		}

		// Load data into IDataView 
		var balancedDataView = mlContext.Data.LoadFromEnumerable(balancedTrainingData);

		// Preprocessing pipeline and build the modle with SdcaLogisticRegression algo for each restaurants lable
		foreach (var restaurant in restaurantColumns)
		{
			var pipeline = mlContext.Transforms.Categorical.OneHotEncoding("TripTypeEncoded", nameof(ActivityTouristInteraction.TripType))
			.Append(mlContext.Transforms.Categorical.OneHotEncoding("BudgetEncoded", nameof(ActivityTouristInteraction.Budget)))
			.Append(mlContext.Transforms.NormalizeMeanVariance(nameof(ActivityTouristInteraction.Travel_time)))
			.Append(mlContext.Transforms.Conversion.ConvertType(nameof(ActivityTouristInteraction.Duration), outputKind: DataKind.Single)) // Convert Duration to float
			.Append(mlContext.Transforms.NormalizeMeanVariance(nameof(ActivityTouristInteraction.Duration))) // Normalize Duration
			.Append(mlContext.Transforms.Concatenate("Features",
			"TripTypeEncoded",
			"BudgetEncoded",
			nameof(ActivityTouristInteraction.Travel_time),
			nameof(ActivityTouristInteraction.Duration))
			.Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
			labelColumnName: restaurant,
			featureColumnName: "Features"

			)));

			var model = pipeline.Fit(balancedDataView);

			// Save the model for future predictions
			mlContext.Model.Save(model, balancedDataView.Schema, $"{restaurant}_Model.zip");

		}


		// Predict recommended restaurants for a new plan

		// Store the new plan data 
		var newTouristInteraction = new RestaurantTouristInteraction
		{
			Duration = (plan.DateEnd.DayNumber - plan.DateBegin.DayNumber) + 1,
			TripType = plan.TripType ?? "Family",
			Travel_time = (float)(plan.TravelTime ?? 10),
			Budget = "$$",
		};

		// Predict and rank accommodations
		var selectedRestaurantIds = new List<int>();

		foreach (var restaurant in restaurantColumns)
		{
			// Load the model for the current restaurant
			var model = mlContext.Model.Load($"{restaurant}_Model.zip", out var inputSchema);
			var predictionEngine = mlContext.Model.CreatePredictionEngine<RestaurantTouristInteraction, RecommendationPrediction>(model);

			// Predict the score for the current restaurant
			var prediction = predictionEngine.Predict(newTouristInteraction);
			var restaurantId = int.Parse(restaurant.Replace("Restaurant", ""));

			// Add to the list only if the score meets the threshold
			if (prediction.Score >= 0.6) // threshold 0.6
			{
				selectedRestaurantIds.Add(restaurantId);
			}
		}

		int recommendationsNum = newTouristInteraction.Duration * 4;

		// Retrieve all restaurants from the database
		var allRestaurantId = db.Restaurants.ToList();

		// Separate selected and non-selected restaurants
		var selectedRestaurants = allRestaurantId
			.Where(x => selectedRestaurantIds.Contains(x.RestaurantId))
			.OrderByDescending(x => x.Rate) // Order by rate descending
			.ToList();

		var nonSelectedRestaurants = allRestaurantId
			.Where(x => !selectedRestaurantIds.Contains(x.RestaurantId))
			.OrderByDescending(x => x.Rate)
			.ToList();


		// Combine selected and non-selected to complete the required number of recommendations
		var finalRestaurants = new List<int>();
		if (recommendationsNum >= 14)
		{
			finalRestaurants = selectedRestaurants
		   .Concat(nonSelectedRestaurants)
		   .OrderByDescending(ac => ac.Rate)
		   .Select(ac => ac.RestaurantId)
		   .ToList();
		}
		else
		{
			finalRestaurants = selectedRestaurants
		   .Concat(nonSelectedRestaurants)
		   .OrderByDescending(ac => ac.Rate)
		   .Take(recommendationsNum)
		   .Select(c => c.RestaurantId)
		   .ToList();

		}

		return finalRestaurants;
	}


}
