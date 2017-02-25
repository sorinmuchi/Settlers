﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public static class MoveManager {

	// Move a knight from source to target
	public static bool moveKnight(Vertex source, Vertex target, Enums.Color color, int currentLongest) {

		// Check if the knight can be moved
		if (!MoveAuthorizer.canKnightMove (source, target, color)) {
			return false;
		}

		Knight sourceKnight = (Knight)source.getOccupyingPiece ();

		// Move the knight
		source.setOccupyingPiece (null);
		target.setOccupyingPiece (sourceKnight);

		// Deactivate the knight
		sourceKnight.deactivateKnight ();

		// Check if longest route needs to be updated

		return true;
	}

	// Displace a knight at target with knight at source
	public static bool displaceKnight(Vertex source, Vertex target, Enums.Color color) {
		
		// Check if the knight can displace
		if (!MoveAuthorizer.canKnightDisplace (source, target, color)) {
			return false;
		}
			
		Knight sourceKnight = (Knight)source.getOccupyingPiece ();
		Knight targetKnight = (Knight)target.getOccupyingPiece ();

		// Move the knight
		source.setOccupyingPiece (null);

		Vertex displacedTarget = new Vertex(Enums.TerrainType.LAND); // = forcedDisplacement(target);
		target.setOccupyingPiece (sourceKnight);
		displacedTarget.setOccupyingPiece (targetKnight);

		// Deactivate the knight
		sourceKnight.deactivateKnight ();

		// Check if longest route needs to be updated

		return true;
	}

	// Upgrade a knight at vertex v
	public static bool upgradeKnight(Dictionary<Enums.ResourceType, int> resources, 
		Dictionary<Enums.DevChartType, int> devChart, Vertex v) {

		return true;
	}

	// Activate a knight at vertex v
	public static bool activateKnight(Dictionary<Enums.ResourceType, int> resources, Vertex v) {
		return true;
	}

	// Upgrade a development chart in the specified field
	public static bool upgradeDevChart(Enums.DevChartType dev, Dictionary<Enums.CommodityType, int> commodities, 
		List<GamePiece> pieces, Dictionary<Enums.DevChartType, int> devChart) {

		return true;
	}

	// Build a settlement at location
	public static bool buidSettlement(Vertex location, Dictionary<Enums.ResourceType, int> resources,
		List<GamePiece> pieces, Enums.Color color) {

		if (!MoveAuthorizer.canBuildSettlement (location, resources, pieces, color)) {
			return false;
		}

		//location.setOccupyingPiece (p);
		//p.putOnBoard ();

		return true;
		return true;
	}


	// Build a city at location
	public static bool buildCity(Vertex location, Dictionary<Enums.ResourceType, int> resources,
		List<GamePiece> pieces, Enums.Color color) {

		return true;
	}

	// Check if a city wall can be built at a vertex
	public static bool buildCityWall(Vertex location, Dictionary<Enums.ResourceType, int> resources,
		int cityWalls, Enums.Color color) {

		return true;
	}

	// Build a knight at location
	public static bool buildKnight(Vertex location, Dictionary<Enums.ResourceType, int> resources,
		List<GamePiece> pieces, Enums.Color color) {

		return true;
	}

	// Build a road at location
	public static bool buildRoad(Edge location, Dictionary<Enums.ResourceType, int> resources,
		List<GamePiece> pieces, Enums.Color color) {

		return true;
	}

	// Build a ship at location
	public static bool buildShip(Edge location, Dictionary<Enums.ResourceType, int> resources,
		List<GamePiece> pieces, Enums.Color color) {

		return true;
	}

	// Move a ship from source to target
	public static bool moveShip(Edge source, Edge target, Enums.Color color) {
		return true;
	}

	// Chase robber from source
	public static bool chaseRobber(Vertex source) {
		return true;
	}

	// Move robber to target
	public static bool moveRobber(Hex target) {
		return true;
	}

	// Move Pirate to target
	public static bool movePirate(Hex target) {
		return true;
	}

	// Place Merchant at target
	public static bool placeMerchant(Hex target) {



		return true;
	}

	// Place an initial settlement
	public static bool placeInitialSettlement (Vertex v, List<GamePiece> pieces) {
		if (!MoveAuthorizer.canPlaceInitialTownPiece (v)) {
			return false;
		}
			
		Settlement settlement = Settlement.getFreeSettlement (pieces);
		v.setOccupyingPiece (settlement);
		settlement.putOnBoard ();

		return true;
	}

	// Place an initial city
	public static bool placeInitialCity (Vertex v, List<GamePiece> pieces) {
		if (!MoveAuthorizer.canPlaceInitialTownPiece (v)) {
			return false;
		}

		City city = City.getFreeCity (pieces);
		v.setOccupyingPiece (city);
		city.putOnBoard ();

		return true;
	}

	// Place an initial road
	public static bool placeInitialRoad (Edge e, Enums.Color color, List<GamePiece> pieces) {
		if (!MoveAuthorizer.canPlaceInitialRoad (e, color)) {
			return false;
		}

		Road road = Road.getFreeRoad (pieces);
		e.setOccupyingPiece (road);
		road.putOnBoard ();

		return true;
	}
	 
	// Place an initial ship
	public static bool placeInitialShip (Edge e, Enums.Color color, List<GamePiece> pieces) {
		if (!MoveAuthorizer.canPlaceInitialShip (e, color)) {
			return false;
		}
			
		Road ship = Road.getFreeShip (pieces);
		e.setOccupyingPiece (ship);
		ship.putOnBoard ();

		return true;
	}
}
