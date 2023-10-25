
# Gatekeeper

The Gatekeeper component is a powerful tool designed to manage the flow of data within Grasshopper for Rhino. It acts as a conditional gate that can prevent data from propagating further in a Grasshopper definition based on a boolean condition, without triggering a recomputation of the solution. This component is particularly useful when you need to control the data flow based on specific conditions, and it ensures a seamless user experience by retaining the previous data state.


## Features

**Conditional Data Flow:** The Gatekeeper component takes a boolean input and a data tree. It forwards the data tree only when the boolean input is true. If the boolean input is false, the component prevents data from flowing forward.

**No Recomputation:** Unlike other conditional components, the Gatekeeper does not retrigger the Grasshopper solution when the boolean input is false. This can significantly improve performance by avoiding unnecessary recomputations.

**Retains Previous Data:** The Gatekeeper allows users to access the previous data from the component even when the boolean condition is false. This feature is especially valuable when you need to reference or analyze data based on various conditions.


## Usage

Connect your data tree to the Gatekeeper component's input.
Connect a boolean value (true or false) to the Gatekeeper's conditional input.
The Gatekeeper will pass the data tree forward only when the boolean input is true.
Even when the boolean input is false, you can access the previous data from the component.
## Installation

You can easily integrate the Gatekeeper component into your Grasshopper workflow using the Grasshopper Package Manager inside Rhino.

Open Grasshopper in Rhino.
Go to the Package Manager.
Search for "Gatekeeper" and install it.
The Gatekeeper component will now be available for use in your Grasshopper definitions.
## Dependencies

This component requires Rhino and Grasshopper to be installed on your system.
## Contributing

If you encounter any issues or have suggestions for improvements, please report them on my GitHub repository.


## Support

For support and questions, feel free to reach out to us at [harrikari.atte@gmail.com].


## GitHub

[@Attemainio](https://www.github.com/attemainio)
## License

[MIT](https://choosealicense.com/licenses/mit/)
