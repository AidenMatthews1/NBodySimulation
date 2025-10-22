# Positioning
Positioning uses long as it was important that there is no floating point error for collisions and interactions and long was required to get the precision wanted over a large enough size 

# Magnitude
Magnitudes use decimal where possible as decimal can more accurately represent values that are bounded in axis limits by the long underlying position.
In rare cases magnitudes need to be squared in this case they can be represented as doubles

# Mass 
Mass is represented as a double. 
A float would be able to store the expected range of masses fine however often masses will need to change by small values ALSO for interactions masses will sometimes need to be multiplied.
To prevent constant type conversions and accuracy loss when manupilating by small values a double was used 
