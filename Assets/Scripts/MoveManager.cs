﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using UnityEngine.Networking;

public class MoveManager : NetworkBehaviour {
    
    GameManager gameManager;
    PrefabHolder prefabHolder;
    Bank bank;
    BoardState boardState;

    void Start()
    {
        gameManager = GetComponent<GameManager>();
        bank = GetComponent<Bank>();
        boardState = GetComponent<BoardState>();
        prefabHolder = GetComponent<PrefabHolder>();
    }

    // Move a knight from source to target
    public bool moveKnight(Vertex source, Vertex target, Enums.Color color, int currentLongest) {

		// Check if the knight can be moved
		if (!MoveAuthorizer.canKnightMove (source, target, color)) {
			return false;
		}

        CmdMoveKnight(source.transform.position, target.transform.position);

		// Check if longest route needs to be updated

		return true;
	}

    [Command]
    void CmdMoveKnight(Vector3 source, Vector3 target)
    {
        RpcMoveKnight(source, target);
    }

    [ClientRpc]
    void RpcMoveKnight(Vector3 source, Vector3 target)
    {
        Vertex sourcePiece = boardState.vertexPosition[source];
        Vertex targetPiece = boardState.vertexPosition[target];

        Knight sourceKnight = (Knight)sourcePiece.getOccupyingPiece();

        // Move the knight
        sourcePiece.setOccupyingPiece(null);
        targetPiece.setOccupyingPiece(sourceKnight);

        // Deactivate the knight
        sourceKnight.deactivateKnight();
    }

	// Displace a knight at target with knight at source
	public bool displaceKnight(Vertex source, Vertex target, Enums.Color color) {
		
		// Check if the knight can displace
		if (!MoveAuthorizer.canKnightDisplace (source, target, color)) {
			return false;
		}

        CmdDisplaceKnight(source.transform.position, target.transform.position);
		// Check if longest route needs to be updated

		return true;
	}

    [Command]
    void CmdDisplaceKnight(Vector3 source, Vector3 target)
    {
        RpcDisplaceKnight(source, target);
    }

    [ClientRpc]
    void RpcDisplaceKnight(Vector3 source, Vector3 target)
    {
        Vertex sourcePiece = boardState.vertexPosition[source];
        Vertex targetPiece = boardState.vertexPosition[target];

        Knight sourceKnight = (Knight)sourcePiece.getOccupyingPiece();
        Knight targetKnight = (Knight)targetPiece.getOccupyingPiece();
        
        // Move the knight
        sourcePiece.setOccupyingPiece(null);

        Vertex displacedTarget = new Vertex(Enums.TerrainType.LAND); // = forcedDisplacement(target);

        targetPiece.setOccupyingPiece(sourceKnight);
        displacedTarget.setOccupyingPiece(targetKnight);

        // Deactivate the knight
        sourceKnight.deactivateKnight();
    }

	// Upgrade a knight at vertex v
	public bool upgradeKnight(int[] resources, int[] devChart, Vertex v) {
        CmdUpgradeKnight(resources, devChart, v.transform.position);
		return true;
	}

    [Command]
    void CmdUpgradeKnight(int[] resources, int[] devChart, Vector3 v)
    {
        RpcUpgradeKnight(resources, devChart, v);
    }

    [ClientRpc]
    void RpcUpgradeKnight(int[] resources, int[] devChart, Vector3 v)
    {

    }

	// Activate a knight at vertex v
	public bool activateKnight(int[] resources, Vertex v) {
        CmdActivateKnight(resources, v.transform.position);
		return true;
	}

    [Command]
    void CmdActivateKnight(int[] resources, Vector3 v)
    {
        RpcActivateKnight(resources, v);
    }

    [ClientRpc]
    void RpcActivateKnight(int[] resources, Vector3 v)
    {

    }

	// Upgrade a development chart in the specified field
	public bool upgradeDevChart(Enums.DevChartType dev, int[] commodities, 
		List<GamePiece> pieces, int[] devChart) {
        
		return true;
	}

	// Build a settlement at location
	public bool buidSettlement(Vertex location, int[] resources,
		List<GamePiece> pieces, Enums.Color color) {

		if (!MoveAuthorizer.canBuildSettlement (location, resources, pieces, color)) {
			return false;
		}

        CmdBuildSettlement(location.transform.position);

        Player current = gameManager.getCurrentPlayer();

        // Add an appropriate amount of victory points
        current.incrementVictoryPoints (1);
		current.incrementVictoryPoints (location.getChits ());

        // Spend the correct resources
        current.discardResource (Enums.ResourceType.BRICK, 1);
		bank.depositResource (Enums.ResourceType.BRICK, 1);

		current.discardResource (Enums.ResourceType.GRAIN, 1);
		bank.depositResource (Enums.ResourceType.GRAIN, 1);

		current.discardResource (Enums.ResourceType.WOOL, 1);
		bank.depositResource (Enums.ResourceType.WOOL, 1);

		current.discardResource (Enums.ResourceType.LUMBER, 1);
		bank.depositResource (Enums.ResourceType.LUMBER, 1);

        /*
         * 
         * 
         *network this
         */
		// Check if there is a port
		updatePort (location);

		// Update longest road

		return true;
	}

    [Command]
    void CmdBuildSettlement(Vector3 location)
    {
        RpcBuildSettlement(location);

        GameObject newSettlement = Instantiate<GameObject>(prefabHolder.settlement, location, Quaternion.identity);
        boardState.spawnedObjects.Add(location, newSettlement);
        NetworkServer.Spawn(newSettlement);
    }

    [ClientRpc]
    void RpcBuildSettlement(Vector3 location)
    {
        Vertex source = boardState.vertexPosition[location];

        List<GamePiece> pieces = gameManager.getCurrentPlayer().getGamePieces();

        // Put a settlement on the board
        Settlement settlement = Settlement.getFreeSettlement(pieces);

        source.setOccupyingPiece(settlement);
        settlement.putOnBoard();
    }


	// Build a city at location
	public bool buildCity(Vertex location, int[] resources,
		List<GamePiece> pieces, Enums.Color color) {

		if (!MoveAuthorizer.canBuildCity (location, resources, pieces, color)) {
			return false;
		}

        CmdBuildCity(location.transform.position);

		// Add an appropriate amount of victory points
		Player current = gameManager.getCurrentPlayer ();
		current.incrementVictoryPoints (1);

		// Spend the resources
		current.discardResource (Enums.ResourceType.GRAIN, 2);
		bank.depositResource (Enums.ResourceType.GRAIN, 2);

		current.discardResource (Enums.ResourceType.ORE, 3);
		bank.depositResource (Enums.ResourceType.ORE, 3);

		return true;
	}

    [Command]
    void CmdBuildCity(Vector3 location)
    {
        RpcBuildCity(location);

        GameObject spawnedCity = Instantiate<GameObject>(prefabHolder.city, location, Quaternion.identity);
        boardState.spawnedObjects.Add(location, spawnedCity);
        NetworkServer.Spawn(spawnedCity);
    }

    [ClientRpc]
    void RpcBuildCity(Vector3 location)
    {
        // Remove the current settlement
        Vertex source = boardState.vertexPosition[location];
        GamePiece settlement = source.getOccupyingPiece();
        
        settlement.takeOffBoard();

        // Build a city
        List<GamePiece> pieces = gameManager.getCurrentPlayer().getGamePieces();
        City city = City.getFreeCity(pieces);

        source.setOccupyingPiece(city);
        city.putOnBoard();
    }

	// Check if a city wall can be built at a vertex
	public bool buildCityWall(Vertex location, int[] resources,
		int cityWalls, Enums.Color color) {
        CmdBuildCityWall(location.transform.position);
		return true;
	}

    [Command]
    void CmdBuildCityWall(Vector3 location)
    {
        RpcBuildCityWall(location);

        GameObject spawnedCityWall = Instantiate<GameObject>(prefabHolder.cityWall, location, Quaternion.identity);
        boardState.spawnedObjects.Add(location, spawnedCityWall);
        NetworkServer.Spawn(spawnedCityWall);
    }

    [ClientRpc]
    void RpcBuildCityWall(Vector3 location)
    {

    }

	// Build a knight at location
	public bool buildKnight(Vertex location, int[] resources,
		List<GamePiece> pieces, Enums.Color color) {
        CmdBuildKnight(location.transform.position);
		return true;
	}

    [Command]
    void CmdBuildKnight(Vector3 location)
    {
        RpcBuildKnight(location);

        GameObject spawnedKnight = Instantiate<GameObject>(prefabHolder.levelOneKnight, location, Quaternion.identity);
        boardState.spawnedObjects.Add(location, spawnedKnight);
        NetworkServer.Spawn(spawnedKnight);
    }

    [ClientRpc]
    void RpcBuildKnight(Vector3 location)
    {

    }

	// Build a road at location
	public bool buildRoad(Edge location, int[] resources,
		List<GamePiece> pieces, Enums.Color color) {

		if (!MoveAuthorizer.canBuildRoad (location, resources, pieces, color)) {
			return false;
		}

        CmdBuildRoad(location.transform.position);

		Player current = gameManager.getCurrentPlayer ();

		// Spend the resources
		current.discardResource (Enums.ResourceType.BRICK, 1);
		bank.depositResource (Enums.ResourceType.BRICK, 1);

		current.discardResource (Enums.ResourceType.LUMBER, 1);
		bank.depositResource (Enums.ResourceType.LUMBER, 1);

		//Update longest route

		return true;
	}

    [Command]
    void CmdBuildRoad(Vector3 location)
    {
        RpcBuildRoad(location);

        GameObject spawnedRoad = Instantiate<GameObject>(prefabHolder.road, location, Quaternion.identity);
        boardState.spawnedObjects.Add(location, spawnedRoad);
        NetworkServer.Spawn(spawnedRoad);
    }

    [ClientRpc]
    void RpcBuildRoad(Vector3 location)
    {
        Edge edge = boardState.edgePosition[location];

        // Put a road on the given edge
        List<GamePiece> pieces = gameManager.getCurrentPlayer().getGamePieces();
        Road road = Road.getFreeRoad(pieces);
        
        edge.setOccupyingPiece(road);
        road.putOnBoard();
        road.wasBuiltThisTurn();
    }

	// Build a ship at location
	public bool buildShip(Edge location, int[] resources,
		List<GamePiece> pieces, Enums.Color color) {

		if (!MoveAuthorizer.canBuildShip (location, resources, pieces, color)) {
			return false;
		}

		// Put a road on the given edge
		Road ship = Road.getFreeShip (pieces);
        /*
         * 
         * NETWORK THIS
         * 
         * 
         */
        location.setOccupyingPiece (ship);
		ship.putOnBoard ();
		ship.wasBuiltThisTurn ();

		Player current = gameManager.getCurrentPlayer ();

		// Spend the resources
		current.discardResource (Enums.ResourceType.WOOL, 1);
		bank.depositResource (Enums.ResourceType.WOOL, 1);

		current.discardResource (Enums.ResourceType.LUMBER, 1);
		bank.depositResource (Enums.ResourceType.LUMBER, 1);

		//Update longest route

		return true;
	}

	// Move a ship from source to target
	public bool moveShip(Edge source, Edge target, Enums.Color color) {
		return true;
	}

	// Chase robber from source
	public bool chaseRobber(Vertex source) {
		return true;
	}

	// Move robber to target
	public bool moveRobber(Hex target) {
		return true;
	}

	// Move Pirate to target
	public bool movePirate(Hex target) {
		return true;
	}

	// Place Merchant at target
	public bool placeMerchant(Hex target) {



		return true;
	}

	// Place an initial settlement
	public bool placeInitialSettlement (Vertex v, List<GamePiece> pieces, List<Hex> validHexes) {
		if (!MoveAuthorizer.canPlaceInitialTownPiece (v, validHexes)) {
			return false;
		}
	
		// Place a settlement
		Settlement settlement = Settlement.getFreeSettlement (pieces);
        /*
         * 
         * NETWORK THIS
         * 
         * 
         */
        v.setOccupyingPiece (settlement);
		settlement.putOnBoard ();

		// Get the resources around the settlement
		Player current = gameManager.getCurrentPlayer ();
		foreach (Hex h in validHexes) {
			if (h.adjacentToVertex (v)) {
				Enums.ResourceType res = gameManager.getResourceFromHex (h.getHexType());
				if (res != Enums.ResourceType.NONE) {
					current.addResource (res, 1);
					bank.withdrawResource (res, 1);
				}
			}
		}

		// Update the victory points and add a port
		current.incrementVictoryPoints (1);
        /*
         * 
         * NETWORK THIS
         * 
         * 
         */
        updatePort(v);

		return true;
	}

	// Place an initial city
	public bool placeInitialCity (Vertex v, List<GamePiece> pieces, List<Hex> validHexes) {
		if (!MoveAuthorizer.canPlaceInitialTownPiece (v, validHexes)) {
			return false;
		}

		// Place a city
		City city = City.getFreeCity (pieces);
        /*
         * 
         * NETWORK THIS
         * 
         * 
         */
        v.setOccupyingPiece (city);
		city.putOnBoard ();

		// Get the resources around the city
		Player current = gameManager.getCurrentPlayer ();
		foreach (Hex h in validHexes) {
			if (h.adjacentToVertex (v)) {
				Enums.ResourceType res = gameManager.getResourceFromHex (h.getHexType());
				if (res != Enums.ResourceType.NONE) {
					current.addResource (res, 1);
					bank.withdrawResource (res, 1);
				}
			}
		}

		// Update the victory points and add a port 
		current.incrementVictoryPoints (2);
        /*
         * 
         * NETWORK THIS
         * 
         * 
         */
        updatePort(v);

		return true;
	}

	// Place an initial road
	public bool placeInitialRoad (Edge e, Enums.Color color, List<GamePiece> pieces) {
		if (!MoveAuthorizer.canPlaceInitialRoad (e, color)) {
			return false;
		}

		// Put a road on the given edge
		Road road = Road.getFreeRoad (pieces);
        /*
         * 
         * NETWORK THIS
         * 
         * 
         */
        e.setOccupyingPiece (road);
		road.putOnBoard ();

		return true;
	}
	 
	// Place an initial ship
	public bool placeInitialShip (Edge e, Enums.Color color, List<GamePiece> pieces) {
		if (!MoveAuthorizer.canPlaceInitialShip (e, color)) {
			return false;
		}
			
		// Put a ship on the given edge
		Road ship = Road.getFreeShip (pieces);
        /*
         * 
         * NETWORK THIS
         * 
         * 
         */
        e.setOccupyingPiece (ship);
		ship.putOnBoard ();

		return true;
	}

	// Get a resource type from a port type
	private Enums.ResourceType getResourceFromPort(Enums.PortType port) {

		switch (port) {
		case Enums.PortType.BRICK:
			return Enums.ResourceType.BRICK;
		case Enums.PortType.WOOL:
			return Enums.ResourceType.WOOL; 
		case Enums.PortType.ORE:
			return Enums.ResourceType.ORE;
		case Enums.PortType.LUMBER:
			return Enums.ResourceType.LUMBER;
		case Enums.PortType.GRAIN:
			return Enums.ResourceType.GRAIN;
		default:
			return Enums.ResourceType.NONE;
		}
	}

	// Update the current players resource ratios according to the given vertex
	private void updatePort(Vertex v) {
		Player current = gameManager.getCurrentPlayer ();
		int[] ratios = current.getResourceRatios ();
		Enums.PortType port = v.getPortType ();

		// If the port is generic, update the ratios accordingly
		if (port == Enums.PortType.GENERIC) {
			for (int i = 0; i < ratios.Length; i++) {
				if (ratios [i] > 3) {
					ratios [i] = 3;
				}
			}
			current.updateResoureRatios (ratios);

		// If there is a specific port, update the correct ratio
		} else if (port != Enums.PortType.NONE) {
			current.updateResourceRatio (getResourceFromPort (port), 2);
		}
	}
    
}
