from setuptools import find_packages, setup

setup(
    name='MeasVReLog',
    version='1.0.0',
    author='Jolly Chen',
    packages=find_packages(),
    include_package_data=True,
    zip_safe=False,
    install_requires=[
        'flask',
        'flask-cors',
        'pillow'
    ],
)