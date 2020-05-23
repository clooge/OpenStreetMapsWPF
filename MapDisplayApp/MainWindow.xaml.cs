﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Itinero;
using System.IO;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using Itinero.LocalGeo;
using System.Globalization;
using MapControl;
using ViewModel;
using MapDisplayApp.Model;
using Newtonsoft.Json;
namespace MapDisplayApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", "XAML Map Control Test Application");
			TileImageLoader.Cache = new MapControl.Caching.ImageFileCache(TileImageLoader.DefaultCacheFolder);
			//TileImageLoader.Cache = new MapControl.Caching.FileDbCache(TileImageLoader.DefaultCacheFolder);
			//TileImageLoader.Cache = new MapControl.Caching.SQLiteCache(TileImageLoader.DefaultCacheFolder);
			//TileImageLoader.Cache = null;

			InitializeComponent();


			ViewModel.Polyline polyline = GetMapControlPolyLineFromOsrmApi();

			AddPolylineToMap(polyline);
		}

		private static void ShowNominatimUsage()
		{
			Nominatim.API.Geocoders.ForwardGeocoder geocoder = new Nominatim.API.Geocoders.ForwardGeocoder();
			Nominatim.API.Models.ForwardGeocodeRequest request = new Nominatim.API.Models.ForwardGeocodeRequest()
			{
				queryString = "Mława, Stary Rynek 1",
				LimitResults = 1,
				ShowGeoJSON = true
			};
			var geocodeResponses = geocoder.Geocode(request);
			geocodeResponses.Wait();
		}

		private static APIHelpers.Coordinate[] GetCoordinatesFromWarszawaToMlawa()
		{
			APIHelpers.Coordinate mlawa = new APIHelpers.Coordinate((float)53.112128, (float)20.383661);
			APIHelpers.Coordinate warszawa = new APIHelpers.Coordinate((float)52.230320, (float)21.011132);
			APIHelpers.Coordinate szczecin = new APIHelpers.Coordinate((float)53.421684, (float)14.561405);
			APIHelpers.Coordinate[] coordinates = new APIHelpers.Coordinate[3];
			coordinates[0] = warszawa;
			coordinates[1] = mlawa;
			coordinates[2] = szczecin;
			return coordinates;
		}

		private static APIHelpers.Coordinate[] GetManyCoordinatesForNormalRoute()
		{
			List<APIHelpers.Coordinate> coordinates = new List<APIHelpers.Coordinate>();

			APIHelpers.Coordinate warszawa = new APIHelpers.Coordinate((float)52.230320, (float)21.011132);	
			APIHelpers.Coordinate bydgoszcz = new APIHelpers.Coordinate((float)53.114519, (float)18.008936);
			APIHelpers.Coordinate lodz = new APIHelpers.Coordinate((float)51.785124, (float)19.462234);
			APIHelpers.Coordinate poznan = new APIHelpers.Coordinate((float)52.398515, (float)16.938702);
			APIHelpers.Coordinate mlawa = new APIHelpers.Coordinate((float)53.112128, (float)20.383661);
			APIHelpers.Coordinate szczecin = new APIHelpers.Coordinate((float)53.421684, (float)14.561405);

			coordinates.Add(warszawa);
			coordinates.Add(bydgoszcz);
			coordinates.Add(lodz);			
			coordinates.Add(poznan);
			coordinates.Add(mlawa);
			coordinates.Add(szczecin);

			return coordinates.ToArray();
		}

		private static APIHelpers.Coordinate[] GetManyCoordinatesForOptimalRoute()
		{
			List<APIHelpers.Coordinate> coordinates = new List<APIHelpers.Coordinate>();

			APIHelpers.Coordinate bydgoszcz = new APIHelpers.Coordinate((float)53.114519, (float)18.008936);
			APIHelpers.Coordinate lodz = new APIHelpers.Coordinate((float)51.785124, (float)19.462234);
			APIHelpers.Coordinate poznan = new APIHelpers.Coordinate((float)52.398515, (float)16.938702);
			APIHelpers.Coordinate mlawa = new APIHelpers.Coordinate((float)53.112128, (float)20.383661);

			coordinates.Add(bydgoszcz);
			coordinates.Add(lodz);
			coordinates.Add(poznan);
			coordinates.Add(mlawa);

			return coordinates.ToArray();
		}

		private void AddPolylineToMap(ViewModel.Polyline polyline)
		{
			(this.DataContext as MapViewModel).Polylines.Add(polyline);
		}

		private static System.Windows.Shapes.Polyline GetPolyline()
		{
			string text = File.ReadAllText(@"C:\OSM\route.geojson");
			GeoJsonModel parsed = JsonConvert.DeserializeObject<GeoJsonModel>(text);
			var polyline = new System.Windows.Shapes.Polyline();
			foreach (Feature feature in parsed.features)
			{
				var startingCoordinates = feature.geometry.coordinates[0] as Newtonsoft.Json.Linq.JArray;
				//
				var startingPoint = new Point((float)startingCoordinates.Last(), (float)startingCoordinates.First());

				var endingCoordinates = feature.geometry.coordinates[1] as Newtonsoft.Json.Linq.JArray;
				//
				var endingPoint = new Point((float)endingCoordinates.Last(), (float)endingCoordinates.First());

				polyline.Points.Add(startingPoint);
				polyline.Points.Add(endingPoint);
			}

			return polyline;
		}

		private static ViewModel.Polyline GetMapControlPolyLine()
		{
			string text = File.ReadAllText(@"C:\OSM\route.geojson");
			GeoJsonModel parsed = JsonConvert.DeserializeObject<GeoJsonModel>(text);
			var polyline = new ViewModel.Polyline();
			polyline.Locations = new MapControl.LocationCollection();
			foreach (Feature feature in parsed.features)
			{
				if (feature.geometry.coordinates[0] is Newtonsoft.Json.Linq.JArray startingCoordinates)
				{
					polyline.Locations.Add(new MapControl.Location((float)startingCoordinates.Last(), (float)startingCoordinates.First()));
				}

				if (feature.geometry.coordinates[1] is Newtonsoft.Json.Linq.JArray endingCoordinates)
				{
					polyline.Locations.Add(new MapControl.Location((float)endingCoordinates.Last(), (float)endingCoordinates.First()));
				}

				//if (feature.geometry.coordinates[0] is double longitude && feature.geometry.coordinates[1] is double latitude)
				//{
				//	polyline.Locations.Add(new MapControl.Location(latitude, longitude));
				//}
			}

			return polyline;
		}

		private static ViewModel.Polyline GetMapControlPolyLineFromOsrmApi()
		{
			//OsrmJsonRouteModel parsed = APIHelpers.OsrmAPIHelper.GetRouteBetweenPoints(GetManyCoordinatesForNormalRoute());

			APIHelpers.Coordinate warszawa = new APIHelpers.Coordinate((float)52.230320, (float)21.011132);
			APIHelpers.Coordinate szczecin = new APIHelpers.Coordinate((float)53.421684, (float)14.561405);
			OsrmJsonRouteModel parsed = APIHelpers.OsrmAPIHelper.GetOptimalRoute(warszawa, szczecin, GetManyCoordinatesForOptimalRoute());

			var polyline = new ViewModel.Polyline();
			polyline.Locations = new MapControl.LocationCollection();
			foreach (var coordinate in parsed.routes[0].geometry.coordinates)
			{
				polyline.Locations.Add(new MapControl.Location((float)coordinate[1], (float)coordinate[0]));

				//if (feature.geometry.coordinates[0] is double longitude && feature.geometry.coordinates[1] is double latitude)
				//{
				//	polyline.Locations.Add(new MapControl.Location(latitude, longitude));
				//}
			}

			return polyline;
		}


		//private static Microsoft.Maps.MapControl.WPF.LocationCollection GetListOfLocations()
		//{
		//	string text = File.ReadAllText(@"C:\GIT\private\Mapsui_mytest\Itinero_test\bin\x64\Debug\route.geojson");
		//	GeoJsonModel parsed = JsonConvert.DeserializeObject<GeoJsonModel>(text);
		//	var locs = new Microsoft.Maps.MapControl.WPF.LocationCollection();

		//	foreach (Feature feature in parsed.features)
		//	{
		//		if (feature.geometry.coordinates[0] is Newtonsoft.Json.Linq.JArray startingCoordinates)
		//		{
		//			//
		//			locs.Add(new Microsoft.Maps.MapControl.WPF.Location((float)startingCoordinates.Last(), (float)startingCoordinates.First()));
		//		}

		//		///var startingPoint = new Point((double)startingCoordinates.Last(), (double)startingCoordinates.First());

		//		if (feature.geometry.coordinates[1] is Newtonsoft.Json.Linq.JArray endingCoordinates)
		//		{
		//			locs.Add(new Microsoft.Maps.MapControl.WPF.Location((float)endingCoordinates.Last(), (float)endingCoordinates.First()));
		//		}

		//		if (feature.geometry.coordinates[0] is double longitude && feature.geometry.coordinates[1] is double latitude)
		//		{
		//			locs.Add(new Microsoft.Maps.MapControl.WPF.Location(latitude, longitude));
		//		}
		//		//
		//		///var endingPoint = new Point((float)endingCoordinates.Last(), (float)endingCoordinates.First());

		//		//polyline.Points.Add(startingPoint);
		//		//polyline.Points.Add(endingPoint);
		//	}

		//	return locs;
		//}

		private static Itinero.Route GetRoute()
		{
			RouterDb routerDb = null;
			using (FileStream stream = new FileInfo(@"C:\GIT\private\map\quebec.routerdb").OpenRead())
			{
				routerDb = RouterDb.Deserialize(stream);
			}
			var router = new Router(routerDb);

			//get a profile
			Itinero.Profiles.Profile profile = Vehicle.Car.Fastest(); //the default OSM car profile


			//routerDb.AddContracted(profile); //dodawanie tego trwa bardzo długo, może się opłacać zrobić to przed wyznaczaniem wielu tras

			var from = new Itinero.LocalGeo.Coordinate(45.532400f, -73.622885f);
			var to = new Itinero.LocalGeo.Coordinate(45.545841f, -73.623474f);

			//create a routerpoint from a location
			//snaps the given location to the nearest routable edge
			RouterPoint start = router.TryResolve(profile, from, 200).Value;
			RouterPoint end = router.TryResolve(profile, to, 200).Value;

			var points = new List<RouterPoint>()
			{
				router.TryResolve(profile, from, 200).Value,
				router.TryResolve(profile, to, 200).Value
			};



			//calculate a route
			//var route = router.Calculate(profile, start, end);
			Result<Itinero.Route> route = router.TryCalculate(profile, points.ToArray());
			return route.Value;
		}




		private void MapMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				//map.ZoomMap(e.GetPosition(map), Math.Floor(map.ZoomLevel + 1.5));
				//map.ZoomToBounds(new BoundingBox(53, 7, 54, 9));
				map.TargetCenter = map.ViewToLocation(e.GetPosition(map));
			}
		}

		private void MapMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				//map.ZoomMap(e.GetPosition(map), Math.Ceiling(map.ZoomLevel - 1.5));
			}
		}

		private void MapMouseMove(object sender, MouseEventArgs e)
		{
			MapControl.Location location = map.ViewToLocation(e.GetPosition(map));
			int latitude = (int)Math.Round(location.Latitude * 60000d);
			int longitude = (int)Math.Round(MapControl.Location.NormalizeLongitude(location.Longitude) * 60000d);
			char latHemisphere = 'N';
			char lonHemisphere = 'E';

			if (latitude < 0)
			{
				latitude = -latitude;
				latHemisphere = 'S';
			}

			if (longitude < 0)
			{
				longitude = -longitude;
				lonHemisphere = 'W';
			}

			mouseLocation.Text = string.Format(CultureInfo.InvariantCulture,
				"{0}  {1:00} {2:00.000}\n{3} {4:000} {5:00.000}",
				latHemisphere, latitude / 60000, (latitude % 60000) / 1000d,
				lonHemisphere, longitude / 60000, (longitude % 60000) / 1000d);
		}

		private void MapMouseLeave(object sender, MouseEventArgs e)
		{
			mouseLocation.Text = string.Empty;
		}

		private void MapManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
		{
			e.TranslationBehavior.DesiredDeceleration = 0.001;
		}

		private void MapItemTouchDown(object sender, TouchEventArgs e)
		{
			var mapItem = (MapItem)sender;
			mapItem.IsSelected = !mapItem.IsSelected;
			e.Handled = true;
		}

		private void SeamarksChecked(object sender, RoutedEventArgs e)
		{
			map.Children.Insert(map.Children.IndexOf(mapGraticule), ((MapViewModel)DataContext).MapLayers.SeamarksLayer);
		}

		private void SeamarksUnchecked(object sender, RoutedEventArgs e)
		{
			map.Children.Remove(((MapViewModel)DataContext).MapLayers.SeamarksLayer);
		}
	}
}
