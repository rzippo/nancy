FROM debian:bullseye

RUN apt-get update && \
    apt-get install -y sudo wget curl libicu67 git        

# Create the non-root user, with sudo access
ARG USER_UID=1000
ARG USER_GID=$USER_UID
RUN groupadd --gid $USER_GID dotnet \
    && useradd --uid $USER_UID --gid $USER_GID -m dotnet \
    && echo dotnet ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/dotnet \
    && chmod 0440 /etc/sudoers.d/dotnet

# Add powershell 7.4, using deb package
USER root
RUN wget https://github.com/PowerShell/PowerShell/releases/download/v7.4.2/powershell_7.4.2-1.deb_amd64.deb
RUN dpkg -i powershell_7.4.2-1.deb_amd64.deb
## Cleanup
RUN rm powershell_7.4.2-1.deb_amd64.deb

# Add dotnet 8.0 and 9.0, using user-local install
USER dotnet
RUN wget https://dot.net/v1/dotnet-install.sh -O /home/dotnet/dotnet-install.sh && \
    chmod +x /home/dotnet/dotnet-install.sh
RUN /home/dotnet/dotnet-install.sh --channel 8.0
RUN /home/dotnet/dotnet-install.sh --channel 9.0
## Cleanup
RUN rm /home/dotnet/dotnet-install.sh

ENV PATH="/home/dotnet/.dotnet:${PATH}"
ENV DOTNET_ROOT="/home/dotnet/.dotnet"

# Add act 0.2.62
USER root
RUN wget https://github.com/nektos/act/releases/download/v0.2.62/act_Linux_x86_64.tar.gz -O /home/dotnet/act_Linux_x86_64.tar.gz
RUN mkdir /usr/local/act && tar -C /usr/local/act -xzf /home/dotnet/act_Linux_x86_64.tar.gz
## Cleanup
RUN rm /home/dotnet/act_Linux_x86_64.tar.gz

ENV PATH="/usr/local/act:${PATH}"

# Set the default user as non-root
USER dotnet
