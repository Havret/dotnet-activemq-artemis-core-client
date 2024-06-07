module.exports = {
  title: '.NET Core Client for ActiveMQ Artemis',
  tagline: 'Unofficial .NET client library for Apache ActiveMQ Artemis utilizing the CORE protocol',
  url: 'https://havret.github.io',
  baseUrl: '/dotnet-activemq-artemis-core-client',
  favicon: 'img/artemis_transparent.png',
  organizationName: 'havret', // Usually your GitHub org/user name.
  projectName: 'dotnet-activemq-artemis-core-client', // Usually your repo name.
  themeConfig: {
    navbar: {
      title: '.NET CORE Client for ActiveMQ Artemis',
      logo: {
        alt: '.NET CORE Client for ActiveMQ Artemis',
        src: 'img/artemis_transparent.png',
      },
      items: [
        {
          to: 'docs/getting-started',
          activeBasePath: 'docs',
          label: 'Docs',
          position: 'right',
        },
        {
          to: 'https://github.com/Havret/dotnet-activemq-artemis-core-client',
          label: 'GitHub',
          position: 'right',
        },
        {
          to: 'https://www.nuget.org/packages/ArtemisNetCoreClient',
          label: 'Download',
          position: 'right',
        }
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Docs',
          items: [
            {
              label: 'Get Started',
              to: 'docs/getting-started',
            }
          ],
        },

        {
          title: 'More',
          items: [            
            {
              label: 'GitHub',
              href: 'https://github.com/Havret/dotnet-activemq-artemis-core-client',
            },
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} Havret. Built with Docusaurus.`,
    },
    prism: {
      additionalLanguages: ['csharp']
    },
    algolia : {
      appId: 'RMXXLJY88T',
      apiKey: 'bdcaf022e9869ccbf8537bbc9bd7e75f',
      indexName: 'dotnet-activemq-artemis-core-client',
      contextualSearch: false
    },
    announcementBar: {
      id: 'supportus',
      content:
        '⭐️ If you like .NET CORE Client for ActiveMQ Artemis, give it a star on <a target="_blank" rel="noopener noreferrer" href="https://github.com/Havret/dotnet-activemq-artemis-core-client">GitHub</a>! ⭐️',
    },
  },
  presets: [
    [
      '@docusaurus/preset-classic',
      {
        docs: {
          path: '../docs',
          sidebarPath: require.resolve('./sidebars.js'),
          routeBasePath: "/docs",
          // Please change this to your repo.
          editUrl:
            'https://github.com/Havret/dotnet-activemq-artemis-core-client/edit/master/website/',
        },
        blog: false,
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      },
    ],
  ],
};
