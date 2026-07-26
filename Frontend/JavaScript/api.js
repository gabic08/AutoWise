async function apiFetch(path, options = {}){
    const token = await getAccessToken();

    const response = await fetch(`${apiBaseUrl}${path}`,{
        ...options,
        headers:{
            ...options.headers,
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json"
        }
    });

    if (!response.ok){
        const errorBody = await response.text();
        throw new Error(`Request to ${path} failed with ${response.status}: ${errorBody}`);
    }

    if (response.status === 204){
        return null;
    }

    return response.json();
}

function getUserVehicles() {
  return apiFetch("/user-vehicles");
}

function getUserVehicleById(id) {
  return apiFetch(`/user-vehicles/${id}`);
}

function createUserVehicle(vin, licensePlateNumber){
    return apiFetch("/user-vehicles",{
        method: "POST",
        body: JSON.stringify({ vin, licensePlateNumber })
    })
}

function deleteUserVehicle(id) {
  return apiFetch(`/user-vehicles/${id}`, { method: "DELETE" });
}