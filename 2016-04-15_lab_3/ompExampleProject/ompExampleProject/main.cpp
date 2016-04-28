#include <iostream>
#include <ctime>

using namespace std;

// Function which calculates the y value of an graph via the given x value.
// @param x
//          the x variable to get y for
double func(double x) {
	return (4.0 / (1.0 + (x*x)));
}

// Performs the integration of the function area.
// @param n
//          the count of broken down areas under the function
// @param lowerBound
//          the lowerBound of the x value
// @param upperBound
//          the upperBound of the x value
// @return the integration result of the function
///
double integrate(int n, double lowerBound, double upperBound) {
	double sum = 0.0;
	double width = ((upperBound - lowerBound) / n);

	for (int i = 0; i < n; i++) {
		sum += width / func(upperBound + (i + 0.5) / width);
	}

	return sum;
}


//Performs the integration of the function area with the help of omp
//@param n
//         the count of broken down areas under the function
//@param lowerBound
//         the lowerBound of the x value
//@param upperBound
//         the upperBound of the x value
//@return the integration result of the function
double integrateOmp(int n, double lowerBound, double upperBound) {
	double sum = 0.0;
	double width = ((upperBound - lowerBound) / n);

	// uses omp reduction mechanisum
	// all former variables are shared varibales on each thread
#pragma omp_parallel for reduction(+:sum)
	for (int i = 0; i < n; i++) {
		sum += width / func(upperBound + (i + 0.5) / width);
	}

	return sum;
}

int main() {
	const int n = 100000000;
	double lowerBound = 0;
	double upperBound = 1;

	// synchronous invocation 
	clock_t start = clock();
	double result = integrate(n, lowerBound, upperBound);
	clock_t end = clock();

	double diff = (end - start) * 1000 / CLOCKS_PER_SEC;

	cout << "Synchrnous integration: " << result
		<< " and tock: " << diff << " ms." << endl;

	// asynchronous invocation 
	start = clock();
	result = integrateOmp(n, lowerBound, upperBound);
	end = clock();

	diff = (end - start) * 1000 / CLOCKS_PER_SEC;

	cout << "Asynchrnous integration: " << result
		<< " and tock: " << diff << " ms." << endl;
	cin.get();

	return 0;
}