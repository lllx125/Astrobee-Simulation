# this code decides which direction to go correspond to which vent to open
import numpy as np
import itertools

# Units in meters (m)
a = ((94.794 + 94.769) / 2) / 1000
b = 69.71 / 1000
c = 139.70 / 1000
d = 50.91 / 1000
e = 56.982 / 1000
f = 139.657 / 1000

# Position of the vents
r = np.array([
    [a,  c,  d],  # 1
    [a,  d,  c],  # 2
    [a, -c, -d],  # 3
    [a, -d, -c],  # 4
    [f,  b, -e],  # 5
    [f, -b,  e],  # 6
    [-a,  c, -d],  # 7
    [-a,  d, -c],  # 8
    [-a, -c,  d],  # 9
    [-a, -d,  c],  # 10
    [-f,  b,  e],  # 11
    [-f, -b, -e],  # 12
])

# Forces in Newtons (N)
F1 = 1.5
F2 = 1.3

F = np.array([
    [0, -F2,  0],  # 1
    [0,  0, -F2],  # 2
    [0,  F2,  0],  # 3
    [0,  0,  F2],  # 4
    [-F1, 0,  0],   # 5
    [-F1, 0,  0],   # 6
    [0, -F2,  0],  # 7
    [0,  0,  F2],  # 8
    [0,  F2,  0],  # 9
    [0,  0, -F2],  # 10
    [F1, 0,  0],   # 11
    [F1, 0,  0],   # 12
])

# Mass in kilograms (kg)
M = 9.12

# Moment of inertia (kg·m²)
I_value = np.array([1.0, 1.0, 1.0])  # Placeholder vector

# Center of mass (m)
COM = np.array([0.0, 0.0, 0.0])

# Calculate torques
T = np.cross(r - COM, F)

# All possible combinations of True/False for 12 vents
bool_combinations = list(itertools.product([True, False], repeat=12))

# determine whether two vectors are parallel


def are_parallel(v1, v2):
    norm1 = np.linalg.norm(v1)
    norm2 = np.linalg.norm(v2)
    if norm1 == 0 and norm2 != 0:
        return False
    elif norm1 != 0 and norm2 == 0:
        return False
    elif norm1 == 0 and norm2 == 0:
        return True
    cross = np.cross(v1, v2)
    return np.allclose(cross, np.zeros(3)) and np.dot(v1, v2) >= 0

# list all vents combination that produces that force and torque


def listVents(Fx=0, Fy=0, Fz=0, Tx=0, Ty=0, Tz=0):
    Force_target = np.array([Fx, Fy, Fz])
    Torque_target = np.array([Tx, Ty, Tz])
    for combo in bool_combinations:
        indices = [j for j in range(12) if combo[j]]
        if not indices:
            continue
        force_sum = np.sum(F[indices], axis=0)
        torque_sum = np.sum(T[indices], axis=0)
        if are_parallel(force_sum, Force_target) and are_parallel(torque_sum, Torque_target):
            print("Active vents:", [j+1 for j in indices])
            print("Force norm: ", np.linalg.norm(force_sum))
            print("Torque norm:", np.linalg.norm(torque_sum))


listVents(Tz=-1)

# results
CONTROL = {
    "+x": [11, 12],
    "-x": [5, 6],
    "+y": [3, 9],
    "-y": [1, 7],
    "+z": [4, 8],
    "-z": [2, 10],
    "+Rx1": [1, 3],  # same torque
    "+Rx2": [8, 10],
    "-Rx1": [2, 4],  # same torque
    "-Rx2": [7, 9],
    "+Ry1": [2, 8],  # bigger torque
    "+Ry2": [5, 11],
    "-Ry1": [4, 10],  # bigger torque
    "-Ry2": [6, 12],
    "+Rz1": [3, 7],  # bigger torque
    "+Rz2": [5, 12],
    "-Rz1": [1, 9],  # bigger torque
    "-Rz2": [6, 11],
}
